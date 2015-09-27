using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using HolidayShow.Data;
using HolidayShowLib;

namespace HolidayShowServer
{
    public class RemoteClient : ByteParserBase
    {
        private readonly TcpClient _client;

        private byte[] _buffer = new byte[2048];

        private int _deviceId = -1;

        public RemoteClient(TcpClient client)
        {
            _client = client;
            base.Parsers.Add(new ParserProtocolContainer(new byte[] { 0x02 }, new byte[] { 0x03 }, 1));
            BeginRead();
        }

        private void BeginRead()
        {
            if (_client.Connected)
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, EndBeginRead, null);
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
                Disconnect();
                return;
            }
            BeginRead();
        }

        public void BeginSend(ProtocolMessage message)
        {
            var bytes = ProtocolHelper.Wrap(message);
            BeginSendBytes(bytes);
        }

        private void BeginSendBytes(byte[] data)
        {
            if (_client.Connected)
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
                Disconnect();
                return;
            }
        }


        private void Disconnect()
        {
            if (_client.Connected)
                _client.Close();
            InvokeOnConnectionClosed();
        }


        public async override void ProcessPacket(byte[] bytes, ParserProtocolContainer parser)
        {
            // get the message
            var message = ProtocolHelper.UnWrap(bytes);
            if (message == null) return;

            if (message.MessageEvent == MessageTypeIdEnum.Unknown) return;

            if (message.MessageEvent == MessageTypeIdEnum.DeviceId)
            {
                if (message.MessageParts.ContainsKey(ProtocolMessage.DEVID))
                {
                    int id = 0;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.DEVID], out id);
                    if (!parsed) return;


                    // Update the pins in the database
                    using (var dc = new EfHolidayContext())
                    {
                        var device = dc.Devices.FirstOrDefault(x => x.DeviceId == id);
                        if (device == null)
                        {
                            device = new Devices { DeviceId = id, Name = "New Device" };
                            dc.Devices.Add(device);
                            await dc.SaveChangesAsync();
                        }
                    }

                    DeviceId = id;
                }
                else
                {
                    return; // Dont do any more... we dont have a valid device ID.
                }

                if (message.MessageParts.ContainsKey(ProtocolMessage.PINSAVAIL))
                {
                    int pinsAvail = 0;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINSAVAIL], out pinsAvail);
                    if (!parsed) return;

                    // Update the pins in the database
                    using (var dc = new EfHolidayContext())
                    {
                        var device = dc.Devices.FirstOrDefault(x => x.DeviceId == DeviceId);
                        if (device == null) return;

                        var ports = device.DeviceIoPorts.ToList();

                        for (int i = 1; i <= pinsAvail; i++)
                        {
                            var port = ports.FirstOrDefault(x => x.CommandPin == i);
                            if (port == null)
                            {
                                port = new DeviceIoPorts {DeviceId = DeviceId, CommandPin = i, Description = "PIN" + i.ToString()};
                                dc.DeviceIoPorts.Add(port);
                            }
                        }

                        // Add a default pin for refernces where no pin is needed.
                        var port1 = ports.FirstOrDefault(x => x.CommandPin == -1);
                        if (port1 == null)
                        {
                            dc.DeviceIoPorts.Add(new DeviceIoPorts(){DeviceId =  DeviceId, CommandPin = -1, Description = "NONE", IsNotVisable = true});
                        }

                        await dc.SaveChangesAsync();
                    }

                }
            }
            if (message.MessageEvent == MessageTypeIdEnum.RequestFile)
            {
                if (message.MessageParts.ContainsKey(ProtocolMessage.FILEDOWNLOAD))
                {
                    // Read from the settings to find the base path.  This is the read path
                    using (var dc = new EfHolidayContext())
                    {
                        var basePathSetting = await dc.Settings.Where(x=> x.SettingName == SettingKeys.FileBasePath).FirstOrDefaultAsync();
                        if (basePathSetting == null || String.IsNullOrWhiteSpace(basePathSetting.ValueString) || !Directory.Exists(basePathSetting.ValueString))
                        {
                            Console.WriteLine("System Setting {0} does not exist in settings table. Must be set to support file transfers", SettingKeys.FileBasePath);
                            BeginSend(new ProtocolMessage(MessageTypeIdEnum.RequestFailed));
                            return;
                        }

                        var fileRequested = message.MessageParts[ProtocolMessage.FILEDOWNLOAD];

                        var combinedPath = Path.Combine(basePathSetting.ValueString, fileRequested);

                        // See if the requested file exists
                        if (!File.Exists(combinedPath))
                        {
                            Console.WriteLine("File Requsted does not exist at path {0}", combinedPath);
                            BeginSend(new ProtocolMessage(MessageTypeIdEnum.RequestFailed));
                            return;
                        }

                        // Send the message data
                        var responseMessage = new ProtocolMessage(MessageTypeIdEnum.RequestFile);
                        responseMessage.MessageParts.Add(ProtocolMessage.AUDIOFILE, fileRequested);
                        responseMessage.MessageParts.Add(ProtocolMessage.FILEBYTES,
                            Convert.ToBase64String(File.ReadAllBytes(combinedPath)));
                        BeginSend(responseMessage);
                    }
                }
            }
        }

        public event EventHandler OnConnectionClosed;

        protected virtual void InvokeOnConnectionClosed()
        {
            EventHandler handler = OnConnectionClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public int DeviceId
        {
            get { return _deviceId; }
            private set
            {
                _deviceId = value;
            }
        }
       
    }
}
