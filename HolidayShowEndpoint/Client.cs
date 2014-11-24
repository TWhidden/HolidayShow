using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using HolidayShowEndpoint.Entities;
using HolidayShowLib;

namespace HolidayShowEndpoint
{
    public class Client : ByteParserBase 
    {
        private readonly IPEndPoint _endPoint;
        private readonly int _deviceId;
        private TcpClient _client;
        private const string AudioFolderName = "AudioFiles";

        private string AudioFilesFullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), AudioFolderName);

        protected static readonly List<AudioContainer> _runningAudioFiles = new List<AudioContainer>();

        private byte[] _buffer = new byte[2048];

        public Client(IPEndPoint endPoint, int deviceId)
        {
            _endPoint = endPoint;
            _deviceId = deviceId;
            _client = new TcpClient();
            base.Parsers.Add(new ParserProtocolContainer(new byte[]{0x02}, new byte[]{0x03}, 1));

            CreateConnection();

        }

        private void CreateConnection()
        {
            Console.WriteLine("Attempting connection to: " + _endPoint.ToString());
            _client.BeginConnect(_endPoint.Address, _endPoint.Port, EndBeginConnect, null);
        }

        private void EndBeginConnect(IAsyncResult a)
        {
            try
            {
                _client.EndConnect(a);

                if (!_client.Connected)
                {
                    Disconnect();
                    return;
                }

                // Setup pin dictionary()
                _rootedTimer.Clear();
                lock (_rootedTimer)
                {
                    foreach (var timer in _rootedTimer)
                    {
                        timer.Value.Dispose();
                    }
                    _rootedTimer.Clear();
                }
                


                BeginRead();


                var dic = new Dictionary<string, string>();
                dic.Add(ProtocolMessage.DEVID, _deviceId.ToString());
                dic.Add(ProtocolMessage.PINSAVAIL, Program.PinsAvailable.Count().ToString());

                var message = new ProtocolMessage(MessageTypeIdEnum.DeviceId, dic);
                BeginSend(message);


            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect.. trying again...");
                Thread.Sleep(1000);
                Disconnect();
            }
        }

        private void BeginRead()
        {
            try
            {
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, EndBeginRead, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error with BeginRead! " + ex.Message);
            }
        }

        private void EndBeginRead(IAsyncResult a)
        {
            try
            {
                var bytesRead = _client.GetStream().EndRead(a);

                if (bytesRead == 0)
                {
                    Disconnect();
                    return;
                }

                var newBuffer = new byte[bytesRead];

                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, bytesRead);

                base.BytesReceived(newBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on EndBeginRead. Error: " + ex.Message);
                ErrorDetectFlash();
                Disconnect();
                return;
            }
            BeginRead();
        }

        public void AllOff()
        {
            // stops all the running audio.
            _runningAudioFiles.ToList().ForEach(x => x.Stop());

            foreach (var broadcomPinNumber in Program.PinsAvailable)
            {
                LibGpio.Gpio.OutputValue(broadcomPinNumber, false);
            }
        }

        private void ErrorDetectFlash()
        {
            AllOff();

            //for (var i = 0; i < 5; i++)
            //{
            //    foreach (var broadcomPinNumber in Program.PinsAvailable)
            //    {
            //        LibGpio.Gpio.OutputValue(broadcomPinNumber, true);
            //    }

            //    Thread.Sleep(250);

            //    foreach (var broadcomPinNumber in Program.PinsAvailable)
            //    {
            //        LibGpio.Gpio.OutputValue(broadcomPinNumber, false);
            //    }
            //    Thread.Sleep(250);
            //}
                
        }

        public void BeginSend(ProtocolMessage message)
        {
            var bytes = ProtocolHelper.Wrap(message);
            BeginSendBytes(bytes);
        }

        private void BeginSendBytes(byte[] data)
        {
            _client.GetStream().BeginWrite(data, 0, data.Length, EndBeginSendBytes, null);
        }

        private void EndBeginSendBytes(IAsyncResult a)
        {
            try
            {
                _client.GetStream().EndWrite(a);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not send data to remote client. Error: " + ex.Message);
                ErrorDetectFlash();
                Disconnect();
            }
        }
        
        
        private void Disconnect()
        {
            if(_client.Connected)
                _client.Close();

            _client = new TcpClient();
            CreateConnection();
        }


        private Dictionary<BroadcomPinNumber, Timer> _rootedTimer = new Dictionary<BroadcomPinNumber, Timer>();


        public override void ProcessPacket(byte[] bytes,
                                           ParserProtocolContainer parser)
        {
            // get the message
            var message = ProtocolHelper.UnWrap(bytes);
            if (message == null) return;

            if (message.MessageEvent == MessageTypeIdEnum.Unknown) return;

            if (message.MessageEvent == MessageTypeIdEnum.KeepAlive)
            {
                var msg = new ProtocolMessage(MessageTypeIdEnum.KeepAlive);
                BeginSend(msg);
            }else if (message.MessageEvent == MessageTypeIdEnum.Reset)
            {
                AllOff();
            }
            else if (message.MessageEvent == MessageTypeIdEnum.EventControl)
            {
                // FOr pin control.
                if (message.MessageParts.ContainsKey(ProtocolMessage.PINID) &&
                    message.MessageParts.ContainsKey(ProtocolMessage.DURATION) &&
                    message.MessageParts.ContainsKey(ProtocolMessage.PINON))
                {
                    int pinId;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINID], out pinId);
                    if (!parsed) return;


                    int durration;
                    parsed = int.TryParse(message.MessageParts[ProtocolMessage.DURATION], out durration);
                    if (!parsed) return;

                    int on;
                    parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINON], out on);
                    if (!parsed) return;

                    var adjustedFor0PinId = pinId - 1;

                    if (adjustedFor0PinId < 0) return;

                    if (adjustedFor0PinId > Program.PinsAvailable.Length) return;


                    Console.WriteLine("Pin {0} start", adjustedFor0PinId);
                    LibGpio.Gpio.OutputValue(Program.PinsAvailable[adjustedFor0PinId], on == 1);

                    if (durration > 0)
                    {
                        lock (_rootedTimer)
                        {
                            if (_rootedTimer.ContainsKey(Program.PinsAvailable[adjustedFor0PinId]))
                            {
                                _rootedTimer[Program.PinsAvailable[adjustedFor0PinId]].Dispose();
                                _rootedTimer.Remove(Program.PinsAvailable[adjustedFor0PinId]);
                            }
                        }
                        Timer timer = null;
                        timer = new Timer((x) =>
                            {

                                Console.WriteLine("Pin {0} Stop", adjustedFor0PinId);
                                LibGpio.Gpio.OutputValue(Program.PinsAvailable[adjustedFor0PinId], false);
                                lock (_rootedTimer)
                                {
                                    if (_rootedTimer.ContainsKey(Program.PinsAvailable[adjustedFor0PinId]))
                                    {
                                        _rootedTimer[Program.PinsAvailable[adjustedFor0PinId]].Dispose();
                                        _rootedTimer.Remove(Program.PinsAvailable[adjustedFor0PinId]);
                                    }
                                }

                            },
                                          null,
                                          TimeSpan.FromMilliseconds(durration),
                                          TimeSpan.FromMilliseconds(-1));
                        lock (_rootedTimer)
                        {
                            if (_rootedTimer.ContainsKey(Program.PinsAvailable[adjustedFor0PinId]))
                            {
                                _rootedTimer[Program.PinsAvailable[adjustedFor0PinId]].Dispose();
                                _rootedTimer[Program.PinsAvailable[adjustedFor0PinId]] = timer;
                            }
                            else
                            {
                                _rootedTimer.Add(Program.PinsAvailable[adjustedFor0PinId], timer);
                            }

                        }


                    }
                }

                if (message.MessageParts.ContainsKey(ProtocolMessage.AUDIOFILE))
                {
                    var audioFile = message.MessageParts[ProtocolMessage.AUDIOFILE];
                    if (!String.IsNullOrWhiteSpace(audioFile))
                    {
                        // See if the file exists in the audio folder
                        var fileLocation = Path.Combine(AudioFilesFullPath, audioFile);
                        if (File.Exists(fileLocation))
                        {
                            AudioContainer ac = null;
                            ac = new AudioContainer(fileLocation);
                            _runningAudioFiles.Add(ac);
                            ac.Start();

                        }
                        else
                        {
                            Console.WriteLine("Could not find audio file {0}", fileLocation);
                        }
                    }

                }

            }
        }

        public class AudioContainer
        {
            private Process _process;

            public AudioContainer(string fileLocation)
            {
                _process = new System.Diagnostics.Process
                {
                    EnableRaisingEvents = false,
                    StartInfo = { FileName = "mpg321", Arguments = string.Format("\"{0}\"", fileLocation) }
                };
                _process.Exited += ProcessExited;
            }

            void ProcessExited(object sender, EventArgs e)
            {
                Console.WriteLine("Audio Process exited");
                _runningAudioFiles.Remove(this);
            }


            public void Start()
            {
                _process.Start();

            }

            public void Stop()
            {
                if (_process != null)
                {
                    try
                    {
                        _process.Kill();
                        _process.WaitForExit(1000);
                        _process.Close();
                    }catch{}
                }
            }

        }
    }

    
}
