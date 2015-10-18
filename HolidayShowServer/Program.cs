using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using HolidayShow.Data;
using HolidayShowLib;


namespace HolidayShowServer
{
    class Program
    {
        private static TcpServer _server; 

        private static bool _running = true;

        private readonly static List<RemoteClient> Clients = new List<RemoteClient>();

        private static readonly Timer UpdateDisplayTimer = new Timer((x)=> UpdateConsole(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        private readonly static List<string> LogMessages = new List<string>();

        private readonly static ConcurrentDictionary<Timer, bool> QueuedTimers = new ConcurrentDictionary<Timer, bool>();

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<InputParams>(args);
            int serverPort = 0;
            var exitCode = result.MapResult
                (
                options => { serverPort = options.ServerPort; return 0; },
                errors => { Console.WriteLine(errors); return 1; }
                );

            if (exitCode == 1) return;

            // Update the database
            using (var dc = new EfHolidayContext())
            {
                dc.UpdateDatabase();
            }

            // Setup the listeners
            _server = new TcpServer((ushort)serverPort);
            _server.OnClientConnected += _server_OnClientConnected;
            _server.Start();

            var t = new Thread(x => RunServer()) { IsBackground = true, Name = "HolidayServer" };
            t.Start();
            

            LogMessage("Press [ENTER] to stop the server");
            Console.ReadLine();
            _running = false;
            _server.Stop();
        }

        static void LogMessage(string message)
        {
            lock (LogMessages)
            {
                LogMessages.Insert(0, $"{DateTime.Now.ToString("HH:mm:ss")}: {message}");
                var count = LogMessages.Count;
                const int maxOutput = 20;
                if (count > maxOutput)
                {
                    LogMessages.RemoveRange(maxOutput, LogMessages.Count - maxOutput);
                }
            }
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

        private async static void RunServer()
        {
            var setExecuting = false;

            LogMessage("3 second Delay to allow connections to come in ...");

            Thread.Sleep(3000);

            LogMessage("Ok, starting.");
            var isOff = false;
            while (_running)
            {
                if (Clients.Count == 0)
                {
                    // no clients, wait for a connection.
                    Thread.Sleep(1000);
                    continue;
                }

                using (var dc = new EfHolidayContext())
                {
                    // check to see if we should be running or not.
                    var schuduleOn = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.OnAt);
                    var schuduleOff = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.OffAt);
                    if (schuduleOn != null && schuduleOff != null && !string.IsNullOrWhiteSpace(schuduleOn.ValueString))
                    {
                        var isAboveBottomTime = IsCurrentTimeGreaterThanUserSettingTime(schuduleOn.ValueString);
                        var isAboveTopTime = IsCurrentTimeGreaterThanUserSettingTime(schuduleOff.ValueString);

                        if (!isAboveBottomTime)
                        {
                            LogMessage("Schudule is currently off");
                            Thread.Sleep(5000);
                            continue;
                        }

                        if (isAboveTopTime)
                        {
                            LogMessage("Schudule is currently off");
                            Thread.Sleep(5000);
                            continue;
                        }
                    }

                    var refresh = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.Refresh);
                    if (refresh != null)
                    {
                        dc.Settings.Remove(refresh);
                        dc.SaveChanges();
                        InitiateReset();
                        setExecuting = false;
                        LogMessage("Reset called and excecuted");
                    }
                }

                try
                {
                    if (!setExecuting)
                    {
                        InitiateReset();
                        setExecuting = true;

                        using (var dc = new EfHolidayContext())
                        {
                            var audioOnAt = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOnAt);
                            var audioOffAt = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOffAt);

                            var isAboveBottomTime = IsCurrentTimeGreaterThanUserSettingTime(audioOnAt.ValueString);
                            var isAboveTopTime = IsCurrentTimeGreaterThanUserSettingTime(audioOffAt.ValueString);

                            var isAudioSchuduleEnabled = (isAboveBottomTime && !isAboveTopTime);

                            // Get the set we should be running.
                            var option = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.SetPlaybackOption);
                            if (option == null)
                            {
                                LogMessage("Set Playback opton not set. Cannot start");
                                Thread.Sleep(1000);
                                goto skipStart;
                            }

                            var setting = (SetPlaybackOptionEnum) option.ValueDouble;

                            var delayBetweenSets = dc.Settings.Where(x => x.SettingName == SettingKeys.DelayBetweenSets).Select(x => (int)x.ValueDouble).FirstOrDefault();
                            if (delayBetweenSets <= 0) delayBetweenSets = 5000;

                            int setId;
                            
                            switch (setting)
                            {
                                case SetPlaybackOptionEnum.Off:
                                    Thread.Sleep(1000);
                                    if (!isOff)
                                    {
                                        
                                        isOff = true;
                                        foreach (var remoteClient in Clients)
                                        {
                                            var di = new DeviceInstructions(remoteClient.DeviceId, MessageTypeIdEnum.Reset);
                                            SendInstruction(di);
                                        }
                                    }
                                    InitiateReset();
                                    setExecuting = false;
                                    goto skipStart;
                                case SetPlaybackOptionEnum.PlaybackRandom:
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

                                    break;
                                case SetPlaybackOptionEnum.PlaybackCurrentOnly:
                                    isOff = false;
                                    // get the set that is currently running
                                    var currentSet =
                                        dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
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

                                    var set = dc.Sets.FirstOrDefault(x => x.SetId == (int)currentSet.ValueDouble && !x.IsDisabled);
                                    if (set == null)
                                    {
                                        LogMessage("Current set references a set that does not exist. Setting to random.");
                                        dc.Settings.Add(new Settings()
                                        {
                                            SettingName = SettingKeys.CurrentSet,
                                            ValueDouble = (int)SetPlaybackOptionEnum.PlaybackRandom,
                                            ValueString = String.Empty
                                        });
                                        await dc.SaveChangesAsync();
                                        goto skipStart;
                                    }

                                    // if we are here, this is a valid set.
                                    setId = set.SetId;


                                    break;
                                default:
                                    {
                                        LogMessage("Invlid SetPlaybackOption. ");
                                        Thread.Sleep(1000);
                                        goto skipStart;
                                    }
                            }

                            // load the set data
                            var setData = dc.Sets.First(x => x.SetId == setId);

                            LogMessage("**************************************");
                            LogMessage("Set: " + setData.SetName);
                            LogMessage("**************************************");

                            // Update the current set playing
                            var current = dc.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                            if (current == null)
                            {
                                current = new Settings()
                                    {
                                        SettingName = SettingKeys.CurrentSet,
                                        ValueString = String.Empty
                                    };
                                dc.Settings.Add(current);
                            }
                            current.ValueDouble = setId;
                            await dc.SaveChangesAsync();

                            // check if audio and danager is enabled
                            var isAudioEnabled =
                                dc.Settings.Where(x => x.SettingName == SettingKeys.IsAudioEnabled)
                                  .Select(x => ((int) x.ValueDouble == 1))
                                  .FirstOrDefault();

                            var isDanagerEnabled =
                                dc.Settings.Where(x => x.SettingName == SettingKeys.IsDanagerEnabled)
                                  .Select(x => ((int)x.ValueDouble == 1))
                                  .FirstOrDefault();

                            var disabledPins = new Dictionary<int, List<int>>();
                            if (!isDanagerEnabled)
                            {
                                disabledPins =
                                    dc.DeviceIoPorts.Where(x => x.IsDanger)
                                        .Select(x => new {x.DeviceId, x.CommandPin})
                                        .GroupBy(x => x.DeviceId)
                                        .ToDictionary(x => x.Key,
                                            y => y.Where(x => x.DeviceId == y.Key).Select(x => x.CommandPin).ToList());
                            }

                            // If the schudule says its off, disable here.
                            if (!isAudioSchuduleEnabled)
                            {
                                isAudioEnabled = false;
                            }

                            // create the work load
                            var deviceInstructions = new List<DeviceInstructions>();

                            foreach (var setSequence in setData.SetSequences.OrderBy(x => x.OnAt))
                            {

                                var deviceId = setSequence.DevicePatterns?.DeviceId;

                                var startingOffset = setSequence.OnAt;

                                if (deviceId.HasValue)
                                {
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
                                                (pattern.DeviceIoPorts.IsDanger && isDanagerEnabled)))
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
                            }

                            var l = new List<int> { 0 };

                            var audioTopDurration =
                                                deviceInstructions.Where(x => x.AudioDuration != 0)
                                                    .OrderByDescending(x => (x.OnAt + x.AudioDuration))
                                                    .Select(x => (x.OnAt + x.AudioDuration))
                                                    .FirstOrDefault();
                            if (audioTopDurration.HasValue)
                                l.Add(audioTopDurration.Value);

                            LogMessage("Top Audio Duration: " + audioTopDurration);

                            var pinTopDurration =
                                deviceInstructions.OrderByDescending(x => (x.OnAt + x.PinDuration))
                                    .Select(x => (x.OnAt + x.PinDuration))
                                    .FirstOrDefault();

                            LogMessage("Top Pin Duration: " + pinTopDurration);

                            if (pinTopDurration.HasValue)
                                l.Add(pinTopDurration.Value);

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
                                // This is something more global, that will be controled at the server side
                                // They are hard-coded effects. at this userTime, baked into the solution. Later, there may be
                                // a plugable architecture but for the sake of userTime, this is all hard coded.

                                if (setSequence.DeviceEffects == null || setSequence.DeviceEffects.EffectInstructionsAvailable.IsDisabled) continue;

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
                                            var data = EffectRandomStrobe(setSequence, totalSetTimeLength, disabledPins);
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
                                foreach (var deviceInstructionse in audio)
                                {
                                    deviceInstructionse.AudioDuration = null;
                                    deviceInstructionse.AudioFileName = string.Empty;
                                }
                            }

                            // once we have a list of to-dos, create the timers to start the sequence.

                            int topDuration = 0;

                            if (deviceInstructions.Count != 0)
                            {
                                for (int index = 0; index < deviceInstructions.Count; index++)
                                {
                                    var di = deviceInstructions[index];
                                    {
                                        var audioTop = di.AudioDuration ?? 0;
                                        var pinTop = di.PinDuration ?? 0;

                                        var top = di.OnAt +
                                                  (pinTop > audioTop ? pinTop : audioTop);
                                        if (top > topDuration)
                                            topDuration = top;

                                        Timer timerStart = null;

                                        timerStart = new Timer(x =>
                                            {
                                                var item = x as DeviceInstructions;

                                                if (item != null)
                                                {
                                                    //LogMessage("Sending Instruction: " + item.ToString());
                                                    SendInstruction(item);
                                                }

                                        
                                                if (timerStart != null)
                                                {
                                                    timerStart.Dispose();
                                                    if (QueuedTimers.ContainsKey(timerStart))
                                                    {
                                                        bool t;
                                                        QueuedTimers.TryRemove(timerStart, out t);
                                                    }
                                                }
                                                
                                            },
                                                               di,
                                                               TimeSpan.FromMilliseconds(di.OnAt),
                                                               TimeSpan.FromMilliseconds(-1));

                                        // Tracks the timer, just incase we need to cancel everything.
                                        QueuedTimers.TryAdd(timerStart, true);
                                    }
                                    if (index == deviceInstructions.Count - 1)
                                    {
                                        // setup the timer to say the set is not executing.
                                        Timer stoppedTimer = null;
                                        stoppedTimer = new Timer(x =>
                                            {
                                                setExecuting = false;
                                                stoppedTimer.Dispose();
                                            },
                                                                 null,
                                                                 TimeSpan.FromMilliseconds(topDuration +
                                                                                           delayBetweenSets),
                                                                 TimeSpan.FromMilliseconds(-1));

                                            QueuedTimers.TryAdd(stoppedTimer, true);
                                    }
                                }
                            }
                            else
                            {
                                LogMessage("No patterns in set. Sleeping for one second");
                                Thread.Sleep(1000);
                                setExecuting = false;
                            }
                        }
                    }

                    skipStart:

                    if (setExecuting)
                        Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    LogMessage("Error occured in server thread! Error: " + ex.Message);
                    Thread.Sleep(1000);
                    setExecuting = false;
                }
            }
        }

        private const string ConsoleDisplayFormat =
            "Holiday Show Version {0}\n" +
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
            var output = string.Format(ConsoleDisplayFormat, version, sb, logMessages);

            Console.Clear();
            Console.Write(output);

        }

        private static bool IsCurrentTimeGreaterThanUserSettingTime(string userTime)
        {
            if (string.IsNullOrWhiteSpace(userTime))
                return false;

            DateTime userTimeObject;

            if (!DateTime.TryParseExact(userTime,
                "HH:mm",
                new CultureInfo("en-US"),
                DateTimeStyles.None,
                out userTimeObject))
            {
                return false;
            }

            return DateTime.Now.TimeOfDay > userTimeObject.TimeOfDay;

        }

        private static List<DeviceInstructions> EffectRandom(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
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

        private static List<DeviceInstructions> EffectRandomStrobe(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50;DELAYBETWEEN=10000
            var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

            var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
            var duration = GetDuration(metadataKeyValue);

            const string keyDelayBetween = "DELAYBETWEEN";
            string delayBetweenStr;
            if (!metadataKeyValue.TryGetValue(keyDelayBetween, out delayBetweenStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyDelayBetween);
                return null;
            }

            uint delayBetween;
            if (!uint.TryParse(delayBetweenStr, out delayBetween))
            {
                LogMessage("EffectRandomStrobe Invalid Value Delay Between: " + delayBetweenStr);
            }

            const string keyExecuteFor = "EXECUTEFOR";
            string executeForStr;
            if (!metadataKeyValue.TryGetValue(keyExecuteFor, out executeForStr))
            {
                LogMessage("EffectRandomStrobe missing " + keyExecuteFor);
                return null;
            }

            uint exectuteFor;
            if (!uint.TryParse(executeForStr, out exectuteFor))
            {
                LogMessage("EffectRandomStrobe Invalid Value Exectue For: " + delayBetweenStr);
            }

            // If the duration was incorrect, null will be returned
            if (duration == null) return null;

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
            var nextDelayAt = startingPoint + exectuteFor;

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
                    nextDelayAt = startingPoint + exectuteFor;
                }
            }

            return list;
        }

        private static List<DeviceInstructions> EffectStayOn(SetSequences setSequence, int? setDurrationMs, Dictionary<int, List<int>> disabledPins)
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

            var timeLeft = setDurrationMs - setSequence.OnAt;

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

        private static int? GetDuration(Dictionary<string, string> metaDataKeyValues)
        {
            const string keyDur = "DUR";

            string dur;
            if (!metaDataKeyValues.TryGetValue(keyDur, out dur))
            {
                LogMessage("DUR= not found in metadata");
                return null;
            }

            if (string.IsNullOrWhiteSpace(dur))
            {
                LogMessage("Pin Duration Expected. Must be formatted like DUR=50");
                return null;
            }

            int duration;
            if (int.TryParse(dur, out duration)) return duration;

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

            string devPins;
            if (!metaDataKeyValue.TryGetValue(keyDevpins, out devPins))
            {
                 LogMessage("Invalid Device:Pin Must be formated like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
                return results;
            }

            if (string.IsNullOrWhiteSpace(devPins))
            {
                LogMessage("Invalid Device:Pin Must be formated like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
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
        private static Random _local;

        public static Random ThisThreadsRandom => _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
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
