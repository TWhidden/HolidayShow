using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CodeSmith.Data.Collections;
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
            if (result.Errors.Any())
            {
                // Values are available here
                Console.WriteLine(result.Errors);
                return;
            }

            // Setup the listeners
            _server = new TcpServer((ushort)result.Value.ServerPort);
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
                    if (schuduleOn != null && schuduleOff != null && !String.IsNullOrWhiteSpace(schuduleOn.ValueString) &&
                        !String.IsNullOrWhiteSpace(schuduleOff.ValueString))
                    {
                        DateTime onTime;

                        var parsed = DateTime.TryParseExact(schuduleOn.ValueString,
                                                            "HH:mm",
                                                            new CultureInfo("en-US"),
                                                            DateTimeStyles.None,
                                                            out onTime);
                        if (parsed)
                        {
                            DateTime offTime;
                            parsed = DateTime.TryParseExact(schuduleOff.ValueString,
                                                            "HH:mm",
                                                            new CultureInfo("en-US"),
                                                            DateTimeStyles.None,
                                                            out offTime);

                            if (parsed)
                            {
                                var currentTime = DateTime.Now;
                                var onTime1 = Convert.ToDateTime(schuduleOn.ValueString);
                                var offTime1 = Convert.ToDateTime(schuduleOff.ValueString);

                                // Schudule is enabled
                                if (DateTime.Compare(currentTime, onTime1) < 0)
                                {
                                    Console.WriteLine("Schudule OFF");
                                    Thread.Sleep(5000);
                                    continue;
                                }


                                if (DateTime.Compare(currentTime, offTime1) > 0)
                                {
                                    Console.WriteLine("Schudule OFF");
                                    Thread.Sleep(5000);
                                    continue;
                                }



                            }

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

                            // create the work load
                            var list = new List<DeviceInstructions>();

                            foreach (var setSequence in setData.SetSequences.OrderBy(x => x.OnAt))
                            {
                                var deviceId = setSequence.DevicePatterns.DeviceId;
                                var startingOffset = setSequence.OnAt;

                                foreach (var pattern in setSequence.DevicePatterns.DevicePatternSequences)
                                {
                                    var onAt = startingOffset + pattern.OnAt;

                                    var di = new DeviceInstructions(deviceId, MessageTypeIdEnum.EventControl)
                                    {
                                        OnAt = onAt
                                    };

                                    bool set = false;
                                    if (!String.IsNullOrWhiteSpace(pattern.AudioOptions?.FileName) && isAudioEnabled)
                                    {
                                        di.AudioFileName = pattern.AudioOptions.FileName;
                                        di.AudioDuration = pattern.AudioOptions.AudioDuration;
                                        set = true;
                                    }

                                    if (pattern.DeviceIoPorts != null && (!pattern.DeviceIoPorts.IsDanger || (pattern.DeviceIoPorts.IsDanger && isDanagerEnabled)))
                                    {
                                        di.CommandPin = pattern.DeviceIoPorts.CommandPin;
                                        di.PinDuration = pattern.Duration;
                                        set = true;
                                    }

                                    if (set)
                                    {
                                        list.Add(di);
                                    }
                                }
                            }

                            // once we have a list of to-dos, create the timers to start the sequence.

                            int topDuration = 0;

                            if (list.Count != 0)
                            {
                                for (int index = 0; index < list.Count; index++)
                                {
                                    var di = list[index];
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
                                    if (index == list.Count - 1)
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

}
