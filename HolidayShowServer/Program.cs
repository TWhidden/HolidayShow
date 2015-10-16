using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            // Setup the listeners
            _server = new TcpServer((ushort)serverPort);
            _server.OnClientConnected += _server_OnClientConnected;
            _server.Start();

            var t = new Thread(x => RunServer()) { IsBackground = true, Name = "HolidayServer" };
            t.Start();
            

            Console.WriteLine("Press [ENTER] to stop the server");
            Console.ReadLine();
            _running = false;
            _server.Stop();
        }

        static void _server_OnClientConnected(object sender, NewClientEventArgs e)
        {
            Console.WriteLine("Client Connected from: " + e.Client.Client.RemoteEndPoint);
            var remoteClient = new RemoteClient(e.Client);
            remoteClient.OnConnectionClosed += remoteClient_OnConnectionClosed;
            Clients.Add(remoteClient);

            
        }

        static void remoteClient_OnConnectionClosed(object sender, EventArgs e)
        {
            var remote = (RemoteClient)sender;
            Console.WriteLine("Connection closed from DeviceId" + remote.DeviceId);
            Clients.Remove((RemoteClient)sender);
        }

        private readonly static  ConcurrentDictionary<Timer, bool> QueuedTimers = new ConcurrentDictionary<Timer, bool>();

        private async static void RunServer()
        {
            var setExecuting = false;

            Console.WriteLine("3 second Delay to allow connections to come in ...");

            Thread.Sleep(3000);

            Console.WriteLine("Ok, starting.");
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
                            Console.WriteLine("Schudule is currently off");
                            Thread.Sleep(5000);
                            continue;
                        }

                        if (isAboveTopTime)
                        {
                            Console.WriteLine("Schudule is currently off");
                            Thread.Sleep(5000);
                            continue;
                        }
                    }
                }

                try
                {
                    if (!setExecuting)
                    {
                        QueuedTimers.Clear();
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
                                Console.WriteLine("Set Playback opton not set. Cannot start");
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
                                    setExecuting = false;
                                    goto skipStart;
                                case SetPlaybackOptionEnum.PlaybackRandom:
                                    // Get all the sets, random set one.
                                    isOff = false;
                                    var sets = dc.Sets.Where(x => !x.IsDisabled).ToList();
                                    if (sets.Count == 0)
                                    {
                                        Console.WriteLine("No sets are configured");
                                        goto skipStart;
                                    }

                                    if (sets.Count == 1)
                                    {
                                        Console.WriteLine("Only one set available.  Setting desired set");
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
                                        Console.WriteLine("Current set is not set. Setting to random");
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
                                        Console.WriteLine("Current set references a set that does not exist. Setting to random.");
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
                                        Console.WriteLine("Invlid SetPlaybackOption. ");
                                        Thread.Sleep(1000);
                                        goto skipStart;
                                    }
                            }

                            // load the set data
                            var setData = dc.Sets.First(x => x.SetId == setId);

                            Console.WriteLine("**************************************");
                            Console.WriteLine("Set: " + setData.SetName);
                            Console.WriteLine("**************************************");

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

                                        if (!string.IsNullOrWhiteSpace(pattern.AudioOptions?.FileName) && isAudioEnabled)
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

                            Console.WriteLine("Top Audio Duration: " + audioTopDurration);

                            var pinTopDurration =
                                deviceInstructions.OrderByDescending(x => (x.OnAt + x.PinDuration))
                                    .Select(x => (x.OnAt + x.PinDuration))
                                    .FirstOrDefault();

                            Console.WriteLine("Top Pin Duration: " + pinTopDurration);

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
                                        var data = EffectRandom(setSequence, totalSetTimeLength);
                                        if (data != null) deviceInstructions.AddRange(data);
                                    }
                                        break;
                                    case EffectsSupported.GPIO_STROBE:
                                    {
                                        var data = EffectStrobe(setSequence, totalSetTimeLength);
                                        if (data != null) deviceInstructions.AddRange(data);
                                    }
                                        break;

                                    case EffectsSupported.GPIO_STAY_ON:
                                        {
                                            var data = EffectStayOn(setSequence, totalSetTimeLength);
                                            if (data != null) deviceInstructions.AddRange(data);
                                        }
                                        break;
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
                                                    Console.WriteLine("Sending Instruction: " + item.ToString());
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
                                Console.WriteLine("No patterns in set. Sleeping for one second");
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
                    Console.WriteLine("Error occured in server thread! Error: " + ex.Message);
                    Thread.Sleep(1000);
                    setExecuting = false;
                }
            }
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

        private static List<DeviceInstructions> EffectRandom(SetSequences setSequence, int? setDurrationMs)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50

            var devicesAndKey = GetDevicesAndPins(setSequence.DeviceEffects.InstructionMetaData);
            var duration = GetDuration(setSequence.DeviceEffects.InstructionMetaData);

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

        private static List<DeviceInstructions> EffectStrobe(SetSequences setSequence, int? setDurrationMs)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50

            var devicesAndKey = GetDevicesAndPins(setSequence.DeviceEffects.InstructionMetaData);
            var duration = GetDuration(setSequence.DeviceEffects.InstructionMetaData);

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

        private static List<DeviceInstructions> EffectStayOn(SetSequences setSequence, int? setDurrationMs)
        {
            // MetaData stored in Effect should be delimited by semi-colons for each instruction set
            // THis Effect will want to know the devices and pin numbers included in the effect,
            // as well as the desired durration
            // Format should be  DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3;DUR=50

            var devicesAndKey = GetDevicesAndPins(setSequence.DeviceEffects.InstructionMetaData);

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

        private static int? GetDuration(string metaData)
        {
            const string KEY_DUR = "DUR=";

            var dataChunks = metaData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var durationStr = dataChunks.FirstOrDefault(x => x.StartsWith(KEY_DUR));

            if (string.IsNullOrWhiteSpace(durationStr))
            {
                Console.WriteLine("Pin Duration Expected. Must be formatted like DUR=50");
                return null;
            }

            int duration;
            if (!int.TryParse(durationStr.Replace(KEY_DUR, ""), out duration))
            {
                Console.WriteLine("Invalid duration: " + durationStr);
                return null;
            }

            return duration;
        }
        

        private static List<KeyValuePair<int, int>> GetDevicesAndPins(string metaData)
        {
            var results = new List<KeyValuePair<int, int>>();

            const string KEY_DEVPINS = "DEVPINS=";

            var dataChunks = metaData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var devPinsStr = dataChunks.FirstOrDefault(x => x.StartsWith(KEY_DEVPINS));

            if (string.IsNullOrWhiteSpace(devPinsStr))
            {
                Console.WriteLine("Invalid Device:Pin Must be formated like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
                return results;
            }

            var subDevPisn = devPinsStr.Replace(KEY_DEVPINS, "");
            var subDevPinAry = subDevPisn.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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

                results.Add(new KeyValuePair<int, int>(deviceId, gpioPinId));
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
                Console.WriteLine("Device {0} - Pin {1} - Duration: {2}", de.DeviceId, de.CommandPin, de.PinDuration);
                dic.Add(ProtocolMessage.PINON, "1");
                dic.Add(ProtocolMessage.DURATION, de.PinDuration.ToString());
                dic.Add(ProtocolMessage.PINID, de.CommandPin.ToString());
            }

            if (!string.IsNullOrWhiteSpace(de.AudioFileName))
            {
                Console.WriteLine("Device {0} - Audio {1} - Duration: {2}", de.DeviceId, de.AudioFileName, de.AudioDuration);
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
        private static Random Local;

        public static Random ThisThreadsRandom => Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
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
