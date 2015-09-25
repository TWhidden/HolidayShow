using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using HolidayShowEndpointUniversalApp;
using HolidayShowLib;

namespace HolidayShowEndpoint
{
    public class Client : ByteParserBase 
    {
        private readonly DnsEndPoint _endPoint;
        private readonly int _deviceId;
        private readonly List<GpioPin> _availablePins;
        private System.Net.Sockets.Socket _client;
        private const string AudioFolderName = "AudioFiles";

        //private string AudioFilesFullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), AudioFolderName);

        protected static readonly List<AudioContainer> _runningAudioFiles = new List<AudioContainer>();
        private GpioController _gpioController;
        private SocketAsyncEventArgs _socketConnectArgs;
        private SocketAsyncEventArgs _socketReceiveArgs;
        private SocketAsyncEventArgs _socketSendArgs;

        public Client(DnsEndPoint endPoint, int deviceId, List<GpioPin> availablePins)
        {
            _endPoint = endPoint;
            _deviceId = deviceId;
            _availablePins = availablePins;
            CreateClientSocket();
            base.Parsers.Add(new ParserProtocolContainer(new byte[]{0x02}, new byte[]{0x03}, 1));

            _gpioController = GpioController.GetDefault();
            if (_gpioController == null)
                return; // GPIO not available on this system

            _socketConnectArgs = new SocketAsyncEventArgs {RemoteEndPoint = _endPoint};
            _socketConnectArgs.Completed += Args_Completed;

            _socketReceiveArgs = new SocketAsyncEventArgs();
            _socketReceiveArgs.Completed += _socketReceiveArgs_Completed;
            var receiveBuffer = new byte[2048];
            _socketReceiveArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

            _socketSendArgs = new SocketAsyncEventArgs();
            _socketSendArgs.Completed += _socketSendArgs_Completed;

            CreateConnection();

        }

        private void CreateClientSocket()
        {
            if(_client != null)
                _client.Dispose();

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }


        private void CreateConnection()
        {
            //Console.WriteLine("Attempting connection to: " + _endPoint.ToString());
            _client.ConnectAsync(_socketConnectArgs);
        }

        private async void Args_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
                

            {
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

                _client.ReceiveAsync(_socketReceiveArgs);

                var dic = new Dictionary<string, string>();
                dic.Add(ProtocolMessage.DEVID, _deviceId.ToString());
                dic.Add(ProtocolMessage.PINSAVAIL, _availablePins.Count.ToString());

                var message = new ProtocolMessage(MessageTypeIdEnum.DeviceId, dic);
                BeginSend(message);


            }
            catch (Exception ex)
            {
                //Console.WriteLine("Could not connect.. trying again...");
                Disconnect();
                await Task.Delay(1000);

            }
        }

        private void _socketReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {

                if (e.BytesTransferred == 0)
                {
                    Disconnect();
                    return;
                }

                //var newBuffer = new byte[e.BytesTransferred];

                //Buffer.BlockCopy(e.Buffer, 0, newBuffer, 0, e.BytesTransferred);

                base.BytesReceived(e.Buffer);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error on EndBeginRead. Error: " + ex.Message);
                ErrorDetectFlash();
                Disconnect();
                return;
            }
            _client.ReceiveAsync(_socketReceiveArgs);
        }

        public void AllOff()
        {
            // stops all the running audio.
            _runningAudioFiles.ToList().ForEach(x => x.Stop());

            for (var i = 0; i < _availablePins.Count; i++)
            {
                var pin = _availablePins[i];
                SetPin(pin, GpioPinValue.Low);
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

        private void SetPin(GpioPin pin, GpioPinValue value)
        {
            Debug.WriteLine("Pin {0} value {1}", pin.PinNumber, value);
            pin.Write(value);
        }

        public void BeginSend(ProtocolMessage message)
        {
            var bytes = ProtocolHelper.Wrap(message);
            BeginSendBytes(bytes);
        }

        private void BeginSendBytes(byte[] data)
        {
            _socketSendArgs.SetBuffer(data, 0, data.Length);
            _client.SendAsync(_socketSendArgs);
        }

        private void _socketSendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            
        }
        
        
        public void Disconnect(bool recreate = true)
        {
            if(_client.Connected)
                _client.Shutdown(SocketShutdown.Both);

            if(recreate)
                CreateClientSocket();
        }


        private Dictionary<GpioPin, Timer> _rootedTimer = new Dictionary<GpioPin, Timer>();


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
                    int pinIndex;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINID], out pinIndex);
                    if (!parsed) return;

                    pinIndex--; // not zero based when its received.

                    int durration;
                    parsed = int.TryParse(message.MessageParts[ProtocolMessage.DURATION], out durration);
                    if (!parsed) return;

                    int on;
                    parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINON], out on);
                    if (!parsed) return;
                    
                    if (pinIndex > _availablePins.Count) return;

                    var gpioPin = _availablePins[pinIndex];

                    //Console.WriteLine("Pin {0} start", adjustedFor0PinId);

                    SetPin(gpioPin, (on == 1) ? GpioPinValue.High : GpioPinValue.Low);

                    if (durration > 0)
                    {
                        lock (_rootedTimer)
                        {
                            if (_rootedTimer.ContainsKey(gpioPin))
                            {
                                _rootedTimer[gpioPin].Dispose();
                                _rootedTimer.Remove(gpioPin);
                            }
                        }
                        Timer timer = null;
                        timer = new Timer((x) =>
                            {

                                //Console.WriteLine("Pin {0} Stop", adjustedFor0PinId);

                                SetPin(gpioPin, GpioPinValue.Low);

                                lock (_rootedTimer)
                                {
                                    if (_rootedTimer.ContainsKey(gpioPin))
                                    {
                                        _rootedTimer[gpioPin].Dispose();
                                        _rootedTimer.Remove(gpioPin);
                                    }
                                }

                            },
                                          null,
                                          TimeSpan.FromMilliseconds(durration),
                                          TimeSpan.FromMilliseconds(-1));
                        lock (_rootedTimer)
                        {
                            if (_rootedTimer.ContainsKey(gpioPin))
                            {
                                _rootedTimer[gpioPin].Dispose();
                                _rootedTimer[gpioPin] = timer;
                            }
                            else
                            {
                                _rootedTimer.Add(gpioPin, timer);
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
                        //var fileLocation = Path.Combine(AudioFilesFullPath, audioFile);
                        //if (File.Exists(fileLocation))
                        //{
                        //    AudioContainer ac = null;
                        //    ac = new AudioContainer(fileLocation);
                        //    _runningAudioFiles.Add(ac);
                        //    ac.Start();

                        //}
                        //else
                        //{
                        //    Console.WriteLine("Could not find audio file {0}", fileLocation);
                        //}
                    }

                }

            }
        }

        public class AudioContainer
        {
            //private Process _process;

            public AudioContainer(string fileLocation)
            {
                //_process = new System.Diagnostics.Process
                //{
                //    EnableRaisingEvents = false,
                //    StartInfo = { FileName = "mpg321", Arguments = string.Format("\"{0}\"", fileLocation) }
                //};
                //_process.Exited += ProcessExited;
            }

            void ProcessExited(object sender, EventArgs e)
            {
               // Console.WriteLine("Audio Process exited");
                _runningAudioFiles.Remove(this);
            }


            public void Start()
            {
               // _process.Start();

            }

            public void Stop()
            {
                //if (_process != null)
                //{
                //    try
                //    {
                //        _process.Kill();
                //        _process.WaitForExit(1000);
                //        _process.Close();
                //    }catch{}
                //}
            }

        }
    }

    
}
