﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using HolidayShow.Data.Core;
using HolidayShowLib;
using HolidayShowLib.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;


namespace HolidayShowServer.Core
{
    class Program
    {
        private static TcpServer _server; 

        private static bool _running = true;

        private static readonly List<RemoteClient> Clients = new List<RemoteClient>();

        private static readonly Timer UpdateDisplayTimer = new Timer((x)=> UpdateConsole(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        private static readonly List<string> LogMessages = new List<string>();

        private static readonly ConcurrentDictionary<Timer, bool> QueuedTimers = new ConcurrentDictionary<Timer, bool>();

        public static string ConnectionString = ConfigurationManager.ConnectionStrings["EfHolidayContext"].ConnectionString;

        private static int _serverPort = -1;

        private static bool _setExecuting = false;

        private static ILogger<Program> _logger;

        static async Task Main(string[] args)
        {


            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("/var/log/HolidayShowServer/server.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
                //.WriteTo.File("c:\\logs\\server1.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Program>>();

            _logger.LogInformation($"STARTUP {DateTime.Now}; Args: {string.Join(" ", args)}");


            var result = Parser.Default.ParseArguments<InputParams>(args);
            _serverPort = 0;
            string dbServer = null;
            string dbName = null;
            string dbUser = null;
            string dbPass = null;
            var exitCode = result.MapResult
            (
                options =>
                {
                    _serverPort = options.ServerPort;
                    dbServer = options.DbServer;
                    dbName = options.DbName;
                    dbPass = options.Password;
                    dbUser = options.Username;
                    return 0;
                },
                errors =>
                {
                    _logger.LogError(string.Join(Environment.NewLine, errors));
                    Console.WriteLine(errors);
                    return 1;
                }
            );

            if (exitCode == 1) return;

            if (!string.IsNullOrWhiteSpace(dbServer))
            {
                ConnectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPass};Trusted_Connection=False;Encrypt=no;";
            }
            _logger.LogInformation($"CONNECTION STRING {ConnectionString}");

            // Update the database
            using (var dc = new EfHolidayContext(ConnectionString))
            {

                _logger.LogInformation("Updating Database...");

                if (Environment.GetEnvironmentVariable("EF_MIGRATION") != "1")
                {
                    await dc.Database.MigrateAsync();
                }

                _logger.LogInformation("Update Complete.");

            }

            // Setup the listeners
            _server = new TcpServer((ushort)_serverPort);
            _server.OnClientConnected += _server_OnClientConnected;
            _server.Start();

            var t = new Thread(x => RunServer()) { IsBackground = true, Name = "HolidayServer" };
            t.Start();

            Thread.Sleep(Timeout.Infinite);

            if (Environment.UserInteractive)
            {
                LogMessage("Press [ENTER] to stop the server");
                Console.ReadLine();
            }
            else
            {
                // Docker has no interactive mode. If there is no console, we need to sleep forever until the task is force quit.
                LogMessage("End Task to stop the server");
                Thread.Sleep(Timeout.Infinite);
            }

            _logger.LogInformation($"SHUTTING DOWN! {DateTime.Now}");

            _running = false;
            _server.Stop();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //we will configure logging here
            services.AddLogging(configure => configure.AddConsole());
            services.AddLogging(configure => configure.AddSerilog());
        }

        public static void LogMessage(string message, bool persistMessage = false)
        {
            lock (LogMessages)
            {
                LogMessages.Insert(0, $"{DateTime.Now:HH:mm:ss}: {message}");
                var count = LogMessages.Count;
                const int maxOutput = 20;
                if (count > maxOutput)
                {
                    LogMessages.RemoveRange(maxOutput, LogMessages.Count - maxOutput);
                }
            }

            if(persistMessage) _logger.LogInformation(message);
        }

        static void _server_OnClientConnected(object sender, NewClientEventArgs e)
        {
            LogMessage("Client Connected from: " + e.Client.Client.RemoteEndPoint);
            var remoteClient = new RemoteClient(e.Client);
            remoteClient.OnConnectionClosed += remoteClient_OnConnectionClosed;
            Clients.Add(remoteClient);
        }

        static void remoteClient_OnConnectionClosed(object sender, EventArgs e)
        {
            var remote = (RemoteClient)sender;
            LogMessage("Connection closed from DeviceId" + remote.DeviceId);
            Clients.Remove((RemoteClient)sender);
        }

        // If the user requests a reset, this will stop all timers, send an all-off, and start over
        static void InitiateReset()
        {
            var timers = QueuedTimers.ToArray();
            foreach (var kv in timers)
            {
                bool v;
                if (QueuedTimers.TryRemove(kv.Key, out v))
                {
                    kv.Key.Dispose();
                }
            }

            // Send messages to each client to Reset
            foreach (var remoteClient in Clients)
            {
                remoteClient.BeginSend(new ProtocolMessage(MessageTypeIdEnum.Reset));
            }
        }

        private static async void RunServer()
        {
            LogMessage("3 second Delay to allow connections to come in ...");

            Thread.Sleep(3000);

            LogMessage("Ok, starting.");
            var isOff = false;
            while (_running)
            {
                try
                {
                    if (Clients.Count == 0)
                    {
                        // no clients, wait for a connection.
                        Thread.Sleep(3000);
                        LogMessage("No Clients Connected!");
                        continue;
                    }

                    using (var dc = new EfHolidayContext(ConnectionString))
                    {
                        var settingKeysOfInterest = new List<string>()
                        {
                            SettingKeys.OnAt,
                            SettingKeys.OffAt,
                            SettingKeys.Refresh
                        };

                        var settings = dc.Settings.Where(x => settingKeysOfInterest.Contains(x.SettingName)).ToList();

                        // check to see if we should be running or not.
                        var scheduleOn = settings.FirstOrDefault(x => x.SettingName == SettingKeys.OnAt);
                        var scheduleOff = settings.FirstOrDefault(x => x.SettingName == SettingKeys.OffAt);
                        if (scheduleOn != null && scheduleOff != null && !string.IsNullOrWhiteSpace(scheduleOn.ValueString))
                        {
                            //var isAboveBottomTime = IsCurrentTimeGreaterThanUserSettingTime(scheduleOn.ValueString);
                            //var isAboveTopTime = IsCurrentTimeGreaterThanUserSettingTime(scheduleOff.ValueString);
                            var isScheduleEnabled =
                                IsCurrentTimeBetweenSettingTimes(scheduleOn.ValueString, scheduleOff.ValueString);

                            if (!isScheduleEnabled)
                            { 

                                LogMessage($"Schedule is currently off. Start: '{scheduleOn.ValueString}' -> '{scheduleOff.ValueString}'; Current Time: '{DateTime.Now.TimeOfDay}'");
                                Thread.Sleep(5000);
                                continue;
                            }
                        }

                        var refresh = settings.FirstOrDefault(x => x.SettingName == SettingKeys.Refresh);
                        if (refresh != null)
                        {
                            dc.Settings.Remove(refresh);
                            dc.SaveChanges();
                            InitiateReset();
                            _setExecuting = false;
                            LogMessage("Reset called and executed");
                        }
                    }
                
                    if (!_setExecuting)
                    {
                        _setExecuting = true;

                        using (var dc = new EfHolidayContext(ConnectionString))
                        {
                            var settingKeysOfInterest = new List<string>()
                            {
                                SettingKeys.AudioOnAt,
                                SettingKeys.AudioOffAt,
                                SettingKeys.SetPlaybackOption,
                                SettingKeys.DelayBetweenSets,
                                SettingKeys.CurrentSet,
                                SettingKeys.DetectDevicePin,
                                SettingKeys.IsAudioEnabled,
                                SettingKeys.IsDangerEnabled,
                            };

                            // Acquire all of the keys needed in this function here.
                            var settings = dc.Settings.Where(x => settingKeysOfInterest.Contains(x.SettingName)).ToList();

                            var audioOnAt = settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOnAt);
                            var audioOffAt = settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOffAt);

                            if (audioOnAt == null)
                            {
                                audioOnAt = new Settings();
                            }

                            if (audioOffAt == null)
                            {
                                audioOffAt = new Settings();
                            }

                            
                            //var isAboveBottomTime = IsCurrentTimeGreaterThanUserSettingTime(audioOnAt.ValueString);
                            //var isAboveTopTime = IsCurrentTimeGreaterThanUserSettingTime(audioOffAt.ValueString);

                            //var isAudioScheduleEnabled = (isAboveBottomTime && !isAboveTopTime);
                            var isAudioScheduleEnabled =
                                IsCurrentTimeBetweenSettingTimes(audioOnAt.ValueString, audioOffAt.ValueString);

                            // Get the set we should be running.
                            var option = settings.FirstOrDefault(x => x.SettingName == SettingKeys.SetPlaybackOption);
                            if (option == null)
                            {
                                LogMessage("Set Playback option not set. Cannot start");
                                Thread.Sleep(1000);
                                goto skipStart;
                            }

                            var setting = (SetPlaybackOptionEnum) option.ValueDouble;

                            var delayBetweenSets = settings.Where(x => x.SettingName == SettingKeys.DelayBetweenSets).Select(x => (int)x.ValueDouble).FirstOrDefault();
                            if (delayBetweenSets <= 0) delayBetweenSets = 5000;

                            int setId;

                            switch (setting)
                            {
                                case SetPlaybackOptionEnum.Off:
                                {
                                    
                                    if (!isOff)
                                    {

                                        isOff = true;
                                        InitiateReset();
                                    }

                                    _setExecuting = false;

                                    Thread.Sleep(1000);
                                    goto skipStart;
                                }
                                case SetPlaybackOptionEnum.PlaybackRandom:
                                {
                                    InitiateReset();
                                    // Get all the sets, random set one.
                                    isOff = false;
                                    var sets = dc.Sets.Where(x => !x.IsDisabled).ToList();
                                    if (sets.Count == 0)
                                    {
                                        LogMessage("No sets are configured");
                                        goto skipStart;
                                    }

                                    if (sets.Count == 1)
                                    {
                                        LogMessage("Only one set available.  Setting desired set");
                                        setId = sets[0].SetId;
                                    }
                                    else
                                    {
                                        var random = new Random();
                                        var index = random.Next(sets.Count);
                                        setId = sets[index].SetId;
                                    }
                                }
                                    break;
                                case SetPlaybackOptionEnum.PlaybackCurrentOnly:
                                {
                                    InitiateReset();
                                    isOff = false;
                                    // get the set that is currently running
                                    var currentSet =
                                        settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                                    if (currentSet == null)
                                    {
                                        LogMessage("Current set is not set. Setting to random");
                                        dc.Settings.Add(new Settings()
                                        {
                                            SettingName = SettingKeys.CurrentSet,
                                            ValueDouble = (int) SetPlaybackOptionEnum.PlaybackRandom,
                                            ValueString = String.Empty
                                        });
                                        await dc.SaveChangesAsync();
                                        goto skipStart;
                                    }

                                    var set =
                                        dc.Sets.FirstOrDefault(
                                            x => x.SetId == (int) currentSet.ValueDouble && !x.IsDisabled);
                                    if (set == null)
                                    {
                                        LogMessage("Current set references a set that does not exist. Setting to random.");

                                        currentSet.SettingName = SettingKeys.CurrentSet;
                                        currentSet.ValueDouble = (int) SetPlaybackOptionEnum.PlaybackRandom;
                                        currentSet.ValueString = string.Empty;

                                        await dc.SaveChangesAsync();
                                        goto skipStart;
                                    }

                                    // if we are here, this is a valid set.
                                    setId = set.SetId;

                                }
                                    break;
                                case SetPlaybackOptionEnum.DevicePinDetect:
                                {
                                    InitiateReset();
                                    var pinDetect = settings.FirstOrDefault(x => x.SettingName == SettingKeys.DetectDevicePin);
                                    if (pinDetect == null)
                                    {
                                        LogMessage("No Detect Device Pin Set");
                                        Thread.Sleep(1000);
                                        continue;
                                    }

                                    // Get the Detect Set
                                    const string SET_DETECT_NAME = "DETECTION SET";
                                    var set = await dc.Sets.Where(x => x.SetName == SET_DETECT_NAME).FirstOrDefaultAsync();
                                    if (set == null)
                                    {
                                        set = new Sets
                                        {
                                            SetName = SET_DETECT_NAME,
                                            IsDisabled = true  // Just keeps this from showing up in the random profile. We ignore this right now.
                                        };
                                        dc.Sets.Add(set);
                                        await dc.SaveChangesAsync();
                                    }
                                    var sequence = set.SetSequences.FirstOrDefault();
                                    if (sequence == null)
                                    {
                                        sequence = new SetSequences {SetId = set.SetId};
                                        dc.SetSequences.Add(sequence);
                                        await dc.SaveChangesAsync();
                                    }

                                    const string EFFECT_DETECT_NAME = "DETECT EFFECT";

                                    var effectDetect = await dc.DeviceEffects.Where(x => x.EffectName == EFFECT_DETECT_NAME).FirstOrDefaultAsync();
                                    if (effectDetect == null)
                                    {
                                        effectDetect = new DeviceEffects
                                        {
                                            EffectName = EFFECT_DETECT_NAME,
                                            Duration = 2000,
                                            EffectInstructionsAvailable = await
                                                dc.EffectInstructionsAvailable.Where(
                                                    x => x.InstructionName == EffectsSupported.GPIO_STROBE)
                                                    .FirstOrDefaultAsync()
                                        };
                                        dc.DeviceEffects.Add(effectDetect);
                                    }

                                    var metaData = $"DEVPINS={pinDetect.ValueString};DUR={500}";

                                    if (effectDetect.InstructionMetaData != metaData)
                                    {
                                        // format: DEVPINS=1:1;DUR=50
                                        effectDetect.InstructionMetaData = metaData;
                                        sequence.DeviceEffects = effectDetect;
                                        await dc.SaveChangesAsync();
                                    }

                                    setId = set.SetId;
                                }

                                    break;
                                default:
                                {
                                    LogMessage("Invalid SetPlaybackOption. ");
                                    Thread.Sleep(1000);
                                    goto skipStart;
                                }
                            }

                            // load the set data
                            var setData = dc.Sets.First(x => x.SetId == setId);

                            LogMessage("**************************************");
                            LogMessage("Set: " + setData.SetName);
                            LogMessage("**************************************");
                            var setBuildSw = Stopwatch.StartNew();

                            // Update the current set playing
                            var current = settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                            if (current == null)
                            {
                                current = new Settings()
                                    {
                                        SettingName = SettingKeys.CurrentSet,
                                        ValueString = String.Empty
                                    };
                                dc.Settings.Add(current);
                            }
                            if (current.ValueDouble != setId)
                            {
                                current.ValueDouble = setId;
                                await dc.SaveChangesAsync();
                            }

                            // check if audio and danger is enabled
                            var isAudioEnabled =
                                settings.Where(x => x.SettingName == SettingKeys.IsAudioEnabled)
                                  .Select(x => ((int) x.ValueDouble == 1))
                                  .FirstOrDefault();

                            var isDangerEnabled =
                                settings.Where(x => x.SettingName == SettingKeys.IsDangerEnabled)
                                  .Select(x => ((int)x.ValueDouble == 1))
                                  .FirstOrDefault();

                            var disabledPins = new Dictionary<int, List<int>>();
                            if (!isDangerEnabled)
                            {
                                disabledPins =
                                    dc.DeviceIoPorts.Where(x => x.IsDanger)
                                        .Select(x => new {x.DeviceId, x.CommandPin})
                                        .GroupBy(x => x.DeviceId)
                                        .ToDictionary(x => x.Key,
                                            y => y.Where(x => x.DeviceId == y.Key).Select(x => x.CommandPin).ToList());
                            }

                            // If the schedule says its off, disable here.
                            if (!isAudioScheduleEnabled)
                            {
                                isAudioEnabled = false;
                            }

                            // create the work load
                            var deviceInstructions = new List<DeviceInstructions>();

                            foreach (var setSequence in setData.SetSequences.OrderBy(x => x.OnAt))
                            {
                                var deviceId = setSequence.DevicePatterns?.DeviceId;

                                var startingOffset = setSequence.OnAt;

                                if (!deviceId.HasValue) continue;
                                foreach (var pattern in setSequence.DevicePatterns.DevicePatternSequences)
                                {
                                    var onAt = startingOffset + pattern.OnAt;

                                    var di = new DeviceInstructions(deviceId.Value, MessageTypeIdEnum.EventControl)
                                    {
                                        OnAt = onAt
                                    };

                                    bool set = false;
                                    if (pattern.DeviceIoPorts != null
                                        && pattern.DeviceIoPorts.CommandPin >= 0
                                        && (!pattern.DeviceIoPorts.IsDanger ||
                                            (pattern.DeviceIoPorts.IsDanger && isDangerEnabled)))
                                    {
                                        di.CommandPin = pattern.DeviceIoPorts.CommandPin;
                                        di.PinDuration = pattern.Duration;
                                        set = true;
                                    }

                                    if (!string.IsNullOrWhiteSpace(pattern.AudioOptions?.FileName))
                                    {
                                        di.AudioFileName = pattern.AudioOptions.FileName;
                                        di.AudioDuration = pattern.AudioOptions.AudioDuration;
                                        set = true;
                                    }

                                    if (set)
                                    {
                                        deviceInstructions.Add(di);
                                    }
                                }
                            }

                            var l = new List<int> { 0 };

                            var audioTopDuration =
                                                deviceInstructions.Where(x => x.AudioDuration != 0)
                                                    .OrderByDescending(x => (x.OnAt + x.AudioDuration))
                                                    .Select(x => (x.OnAt + x.AudioDuration))
                                                    .FirstOrDefault();
                            if (audioTopDuration.HasValue)
                                l.Add(audioTopDuration.Value);

                            LogMessage(audioTopDuration != null
                                ? $"Top Audio Duration: {audioTopDuration}"
                                : "No Audio In set");

                            var pinTopDuration =
                                deviceInstructions.OrderByDescending(x => (x.OnAt + x.PinDuration))
                                    .Select(x => (x.OnAt + x.PinDuration))
                                    .FirstOrDefault();

                            LogMessage(
                                pinTopDuration != null ? 
                                    $"Top Pin Duration: {pinTopDuration}" 
                                    : "No Pins in set");

                            if (pinTopDuration.HasValue)
                                l.Add(pinTopDuration.Value);

                            var totalSetTimeLength = l.Max();

                            if (totalSetTimeLength == 0)
                            {
                                // It may be that just effects are listed, and they have have a desired runtime, so we will 
                                // alter the time to go off the highest time in the set.
                                totalSetTimeLength =
                                    setData.SetSequences.Where(x => x.DeviceEffects != null)
                                        .OrderByDescending(x => (x.OnAt + x.DeviceEffects.Duration))
                                        .Select(x => (x.OnAt + x.DeviceEffects.Duration))
                                        .FirstOrDefault();
                            }


                            foreach (var setSequence in setData.SetSequences.Where(x => x.DeviceEffects != null).OrderBy(x => x.OnAt))
                            {
                                // New Feature added 2015 - Effects
                                // This is something more global, that will be controlled at the server side
                                // They are hard-coded effects. at this userTime, baked into the solution. Later, there may be
                                // a plug-able architecture but for the sake of userTime, this is all hard coded.

                                if (setSequence.DeviceEffects == null || setSequence.DeviceEffects.EffectInstructionsAvailable.IsDisabled) continue;

                                var timeOn = setSequence.DeviceEffects.TimeOn;
                                var timeOff = setSequence.DeviceEffects.TimeOff;

                                if (!string.IsNullOrWhiteSpace(timeOn) && !string.IsNullOrWhiteSpace(timeOff))
                                {
                                    if (!IsCurrentTimeBetweenSettingTimes(timeOn, timeOff))
                                    {
                                        continue;
                                    }
                                }
                                 

                                switch (setSequence.DeviceEffects.EffectInstructionsAvailable.InstructionName)
                                {
                                    case EffectsSupported.GPIO_RANDOM:
                                    {
                                        var data = EffectRandom(setSequence, totalSetTimeLength, disabledPins);
                                        if (data != null) deviceInstructions.AddRange(data);
                                    }
                                        break;
                                    case EffectsSupported.GPIO_STROBE:
                                    {
                                        var data = EffectStrobe(setSequence, totalSetTimeLength, disabledPins);
                                        if (data != null) deviceInstructions.AddRange(data);
                                    }
                                        break;

                                    case EffectsSupported.GPIO_STAY_ON:
                                        {
                                            var data = EffectStayOn(setSequence, totalSetTimeLength, disabledPins);
                                            if (data != null) deviceInstructions.AddRange(data);
                                        }
                                        break;
                                    case EffectsSupported.GPIO_STROBE_DELAY:
                                        {
                                            var data = EffectStrobeDelay(setSequence, totalSetTimeLength, disabledPins);
                                            if (data != null) deviceInstructions.AddRange(data);
                                        }
                                        break;
                                    case EffectsSupported.GPIO_SEQUENTIAL:
                                        {
                                            var data = EffectSequential(setSequence, totalSetTimeLength, disabledPins);
                                            if (data != null) deviceInstructions.AddRange(data);
                                        }
                                        break;
                                    case EffectsSupported.GPIO_RANDOM_DELAY:
                                        {
                                            var data = EffectRandomDelay(setSequence, totalSetTimeLength, disabledPins);
                                            if (data != null) deviceInstructions.AddRange(data);
                                        }
                                        break;
                                }
                            } 

                            // #9 - Now that audio and effects have been added, we need to check to see
                            // if audio is enabled, and if its not, remove any audio.
                            if (!isAudioEnabled)
                            {
                                var audio = deviceInstructions.Where(x => x.AudioDuration > 0).ToList();
                                foreach (var audioInstruction in audio)
                                {
                                    audioInstruction.AudioDuration = null;
                                    audioInstruction.AudioFileName = string.Empty;
                                }
                            }

                            // once we have a list of to-dos, create the timers to start the sequence.

                            var topDuration = 0;

                            if (deviceInstructions.Count != 0)
                            {
                                for (var currentInstructionIndex = 0; currentInstructionIndex < deviceInstructions.Count; currentInstructionIndex++)
                                {
                                    var di = deviceInstructions[currentInstructionIndex];
                                    {
                                        var audioTop = di.AudioDuration ?? 0;
                                        var pinTop = di.PinDuration ?? 0;

                                        var top = di.OnAt +
                                                  (pinTop > audioTop ? pinTop : audioTop);
                                        if (top > topDuration)
                                            topDuration = top;

                                        Timer timerStart = null;

                                        timerStart = new Timer(instructionState =>
                                            {
                                                if (instructionState is DeviceInstructions item)
                                                {
                                                    //LogMessage("Sending Instruction: " + item.ToString());
                                                    SendInstruction(item);
                                                }

                                                if (timerStart == null) return;
                                                timerStart.Dispose();
                                                if (QueuedTimers.ContainsKey(timerStart))
                                                {
                                                    QueuedTimers.TryRemove(timerStart, out _);
                                                }

                                            },
                                            di,
                                            TimeSpan.FromMilliseconds(di.OnAt),
                                            TimeSpan.FromMilliseconds(-1));

                                        // Tracks the timer, just incase we need to cancel everything.
                                        QueuedTimers.TryAdd(timerStart, true);
                                    }
                                    if (currentInstructionIndex != deviceInstructions.Count - 1) continue;
                                    // setup the timer to say the set is not executing.
                                    Timer stoppedTimer = null;
                                    stoppedTimer = new Timer(x =>
                                        {
                                            LogMessage("Set Execution Complete.");
                                            _setExecuting = false;
                                            stoppedTimer.Dispose();
                                        },
                                        null,
                                        TimeSpan.FromMilliseconds(topDuration + delayBetweenSets),
                                        TimeSpan.FromMilliseconds(-1));

                                    QueuedTimers.TryAdd(stoppedTimer, true);
                                }
                            }
                            else
                            {
                                LogMessage("No patterns in set. Sleeping for one second");
                                Thread.Sleep(1000);
                                _setExecuting = false;
                            }

                            LogMessage($"Set Build Time: {setBuildSw.Elapsed}");
                        }
                    }

                    skipStart:

                    // The loop will re-check state ever X Milliseconds
                    if (_setExecuting)
                        Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    LogMessage("Error occured in server thread! Error: " + ex.Message);
                    Thread.Sleep(1000);
                    _setExecuting = false;
                }
            }
        }

        private const string ConsoleDisplayFormat =
            "Holiday Show Version {0}; Port: {3}\n" +
            "-----------------------------------------------------\n" +
            "Connected Endpoints:\n" +
            "{1}" +
            "-----------------------------------------------------\n" +
            "{2}";

        private const string ConnectedEndpointFormat = "{0}:{1} - {2}/s ({3}) Online Since {4}\n";


        /// <summary>
        /// This console will update the display.
        /// </summary>
        private static void UpdateConsole()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Build the Connected Endpoint 
            var sb = new StringBuilder();
            foreach (var c in Clients.ToList().OrderBy(x => x.DeviceId))
            {
                sb.Append(string.Format(ConnectedEndpointFormat, c.DeviceId, c.RemoteAddress, c.MessagesPer(1),
                    c.MessageCountTotal, c.CameOnline));
            }
            string logMessages;
            lock (LogMessages)
            {
                logMessages = string.Join("\n", LogMessages);
            }
            var output = string.Format(ConsoleDisplayFormat, version, sb, logMessages, _serverPort);

            Console.Clear();
            Console.Write(output);

        }

        /// <summary>
        /// Fix to existing time checking - Idea from
        /// https://stackoverflow.com/a/21343435
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private static bool IsCurrentTimeBetweenSettingTimes(string startTime, string endTime)
        {
            if (!TimeSpan.TryParse(startTime, out var start))
            {
                Console.WriteLine($"Could not parse start time: {startTime}");
                return false;
            }

            if (!TimeSpan.TryParse(endTime, out var end))
            {
                Console.WriteLine($"Could not parse end time: {endTime}");
                return false;
            }

            TimeSpan now = DateTime.Now.TimeOfDay;

            if (start <= end)
            {
                // start and stop times are in the same day
                if (now >= start && now <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }
            else
            {
                // start and stop times are in different days
                if (now >= start || now <= end)
                {
                    // current time is between start and stop
                    return true;
                }
            }

            return false;
        }

        private static List<DeviceInstructions> EffectRandom(SetSequences setSequence, int? setDurationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // now that we have a Key/Value Pair list with ints for devices and keys, we can now randomize them
            // and build up the list that we will return

            // Shuffle the list for random
            devicesAndKey.Shuffle();

            // Devide up the desired duration by the number of items
            // for an even distribution 
            var startingPoint = setSequence.OnAt;

            var list = new List<DeviceInstructions>();

            var endingPosition = setDurationMs;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;
            }

            while (startingPoint < endingPosition)
            {
                foreach (var dk in devicesAndKey)
                {
                    if (startingPoint + duration.Value > endingPosition.Value)
                        goto end;

                    var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                    {
                        OnAt = startingPoint,
                        CommandPin = dk.Value,
                        PinDuration = duration
                    };

                    list.Add(di);

                    startingPoint = startingPoint + (duration.Value * 2);
                }

                devicesAndKey.Shuffle();
            }

            end:

            return list;
        }

        private static List<DeviceInstructions> EffectRandomDelay(SetSequences setSequence, int? setDurationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50;DELAYBETWEEN=10000;EXECUTEFOR=2000
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

            const string keyDelayBetween = "DELAYBETWEEN";
            if (!metadataKeyValue.TryGetValue(keyDelayBetween, out var delayBetweenStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyDelayBetween);
                return null;
            }

            if (!uint.TryParse(delayBetweenStr, out var delayBetween))
            {
                LogMessage("EffectRandomStrobe Invalid Value Delay Between: " + delayBetweenStr);
            }

            const string keyExecuteFor = "EXECUTEFOR";
            if (!metadataKeyValue.TryGetValue(keyExecuteFor, out var executeForStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyExecuteFor);
                return null;
            }

            if (!uint.TryParse(executeForStr, out var executeFor))
            {
                LogMessage("EffectRandomStrobe Invalid Value Execute For: " + delayBetweenStr);
            }

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // now that we have a Key/Value Pair list with ints for devices and keys, we can now randomize them
            // and build up the list that we will return

            // Shuffle the list for random
            devicesAndKey.Shuffle();

            // Devide up the desired duration by the number of items
            // for an even distribution 
            long startingPoint = setSequence.OnAt;

            var list = new List<DeviceInstructions>();

            var endingPosition = setDurationMs;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;
            }

            var nextDelayAt = startingPoint + executeFor;

            while (startingPoint < endingPosition)
            {
                foreach (var dk in devicesAndKey)
                {
                    if (startingPoint + duration.Value > endingPosition.Value)
                        goto end;

                    var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                    {
                        OnAt = (int)startingPoint,
                        CommandPin = dk.Value,
                        PinDuration = duration
                    };

                    list.Add(di);

                    startingPoint = startingPoint + (duration.Value * 2);
                    if (startingPoint >= nextDelayAt)
                    {
                        startingPoint = startingPoint + delayBetween;
                        nextDelayAt = startingPoint + executeFor;
                    }
                }

                devicesAndKey.Shuffle();
            }

            end:

            return list;
        }

        private static List<DeviceInstructions> EffectStrobe(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // now that we have a Key/Value Pair list with ints for devices and keys, we can loop them and configure sequences
            // and build up the list that we will return

            var startingPoint = setSequence.OnAt;
            
            var list = new List<DeviceInstructions>();

            var endingPosition = setDurrationMs;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;
            }

            while (startingPoint < endingPosition)
            {
                list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                {
                    OnAt = startingPoint, CommandPin = dk.Value, PinDuration = duration
                }));

                startingPoint = startingPoint + (duration.Value * 2);
            }

            return list;
        }

        private static List<DeviceInstructions> EffectStrobeDelay(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50;DELAYBETWEEN=10000;EXECUTEFOR=2000
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

            const string keyDelayBetween = "DELAYBETWEEN";
            if (!metadataKeyValue.TryGetValue(keyDelayBetween, out var delayBetweenStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyDelayBetween);
                return null;
            }

            if (!uint.TryParse(delayBetweenStr, out var delayBetween))
            {
                LogMessage("EffectRandomStrobe Invalid Value Delay Between: " + delayBetweenStr);
            }

            const string keyExecuteFor = "EXECUTEFOR";
            if (!metadataKeyValue.TryGetValue(keyExecuteFor, out var executeForStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyExecuteFor);
                return null;
            }

            if (!uint.TryParse(executeForStr, out var executeFor))
            {
                LogMessage("EffectRandomStrobe Invalid Value Execute For: " + delayBetweenStr);
            }

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // now that we have a Key/Value Pair list with ints for devices and keys, we can loop them and configure sequences
            // and build up the list that we will return

            long startingPoint = setSequence.OnAt;

            var list = new List<DeviceInstructions>();

            var endingPosition = setDurrationMs;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;
            }
            var nextDelayAt = startingPoint + executeFor;

            while (startingPoint < endingPosition)
            {
                list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                {
                    OnAt = (int)startingPoint,
                    CommandPin = dk.Value,
                    PinDuration = duration
                }));

                startingPoint = startingPoint + (duration.Value * 2);
                if (startingPoint >= nextDelayAt)
                {
                    startingPoint = startingPoint + delayBetween;
                    nextDelayAt = startingPoint + executeFor;
                }
            }

            return list;
        }

        private static List<DeviceInstructions> EffectStayOn(SetSequences setSequence, int? setDurationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // now that we have a Key/Value Pair list with ints for devices and keys, we can loop them and configure sequences
            // and build up the list that we will return

            var startingPoint = setSequence.OnAt;

            var list = new List<DeviceInstructions>();

            var timeLeft = setDurationMs - setSequence.OnAt;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                timeLeft = setSequence.DeviceEffects.Duration;
            }

            if (timeLeft <= 0) return list;
          
            list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
            {
                OnAt = startingPoint,
                CommandPin = dk.Value,
                PinDuration = timeLeft
            }));

            return list;
        }

        private static List<DeviceInstructions> EffectSequential(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            const string reverseKey = "REVERSE";

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

            var reverse = (metadataKeyValue.ContainsKey(reverseKey) && metadataKeyValue[reverseKey] == "1");

            // If there is nothing, return so we dont have a dev by zero
            if (devicesAndKey.Count == 0) return null;

            // Divide up the desired duration by the number of items
            // for an even distribution 
            var startingPoint = setSequence.OnAt;

            var list = new List<DeviceInstructions>();

            var endingPosition = setDurrationMs;

            if (setSequence.DeviceEffects.Duration > 0)
            {
                endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;
            }

            while (startingPoint < endingPosition)
            {
                foreach (var dk in devicesAndKey)
                {
                    if (startingPoint + duration.Value > endingPosition.Value)
                        goto end;

                    var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                    {
                        OnAt = startingPoint,
                        CommandPin = dk.Value,
                        PinDuration = duration
                    };

                    list.Add(di);

                    startingPoint = startingPoint + (duration.Value * 2);
                }
                
                // If the request is to go the other way, this will reverse the order of the keys for the next pass
                if (reverse)
                    devicesAndKey.Reverse();
            }

            end:

            return list;
        }

        private static int? GetDuration(Dictionary<string, string> metaDataKeyValues)
        {
            const string keyDur = "DUR";

            if (!metaDataKeyValues.TryGetValue(keyDur, out var dur))
            {
                LogMessage("DUR= not found in metadata");
                return null;
            }

            if (string.IsNullOrWhiteSpace(dur))
            {
                LogMessage("Pin Duration Expected. Must be formatted like DUR=50");
                return null;
            }

            if (int.TryParse(dur, out var duration)) return duration;

            LogMessage("Invalid duration: " + dur);
            return null;
        }

        private static Dictionary<string, string> GetMetaDataKeyValues(string metaData)
        {
            var dataChunks = metaData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return dataChunks.Select(dataChunk => dataChunk
                .Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries))
                .Where(kvarry => kvarry.Length == 2)
                .ToDictionary(kvarry => kvarry[0], kvarry => kvarry[1]);
        }
        

        private static List<KeyValuePair<int, int>> GetDevicesAndPins(Dictionary<string, string> metaDataKeyValue, Dictionary<int, List<int>> disabledPins)
        {
            var results = new List<KeyValuePair<int, int>>();

            const string keyDevpins = "DEVPINS";

            if (!metaDataKeyValue.TryGetValue(keyDevpins, out var devPins))
            {
                 LogMessage("Invalid Device:Pin Must be formatted like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
                return results;
            }

            if (string.IsNullOrWhiteSpace(devPins))
            {
                LogMessage("Invalid Device:Pin Must be formatted like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
                return results;
            }
            
            var subDevPinAry = devPins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kv in subDevPinAry)
            {
                var devIdPin = kv.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (devIdPin.Length != 2)
                {
                    continue;
                }
                int deviceId;
                int gpioPinId;

                if (!int.TryParse(devIdPin[0], out deviceId))
                {
                    continue;
                }

                if (!int.TryParse(devIdPin[1], out gpioPinId))
                {
                    continue;
                }

                if (!disabledPins.ContainsKey(deviceId) ||
                    (disabledPins.ContainsKey(deviceId) && !disabledPins[deviceId].Contains(gpioPinId)))
                {
                    results.Add(new KeyValuePair<int, int>(deviceId, gpioPinId));
                }
            }

            return results;

        }

        private static void SendInstruction(DeviceInstructions de)
        {
            var client = Clients.FirstOrDefault(x => x.DeviceId == de.DeviceId);
            if (client == null) return;

            var dic = new Dictionary<string, string>();

            if (de.CommandPin != -1)
            {
                //LogMessage("Device {0} - Pin {1} - Duration: {2}", de.DeviceId, de.CommandPin, de.PinDuration);
                dic.Add(ProtocolMessage.PINON, "1");
                dic.Add(ProtocolMessage.DURATION, de.PinDuration.ToString());
                dic.Add(ProtocolMessage.PINID, de.CommandPin.ToString());
            }

            if (!string.IsNullOrWhiteSpace(de.AudioFileName))
            {
                //LogMessage("Device {0} - Audio {1} - Duration: {2}", de.DeviceId, de.AudioFileName, de.AudioDuration);
                dic.Add(ProtocolMessage.AUDIOFILE, de.AudioFileName);
            }

            var message = new ProtocolMessage(de.DeviceCommand, dic);

            client.BeginSend(message);

        }
       
    }

    public class DeviceInstructions
    {

        public DeviceInstructions(int deviceId, MessageTypeIdEnum deviceCommand)
        {
            DeviceId = deviceId;
            DeviceCommand = deviceCommand;
        }

        public MessageTypeIdEnum DeviceCommand { get; }

        public int DeviceId { get; }

        public int OnAt { get; set; }

        public int CommandPin { get; set; }

        public int? PinDuration { get; set; }

        public string AudioFileName { get; set; }

        public int? AudioDuration { get; set; }

        public override string ToString()
        {
            return $"Dev: {DeviceId}; OnAt: {OnAt}, CommandPin{CommandPin}";
        }
    }


    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random? _local;

        public static Random ThisThreadsRandom => _local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

}
