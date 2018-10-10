using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using HolidayShowLib;
#if NETCOREAPP
using HolidayShow.Data;
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using HolidayShow.Data;
#endif

namespace HolidayShowServer
{
    public class RemoteClient : ByteParserBase
    {
        private readonly TcpClient _client;

        private readonly byte[] _buffer = new byte[2048];

        private const int StoreHistoryForSeconds = 10;

        private readonly List<long> _eventHistory = new List<long>();

        private readonly ConcurrentQueue<byte[]> _dataToSendQueue = new ConcurrentQueue<byte[]>();

        public long MessageCountTotal { get; private set; }

        public RemoteClient(TcpClient client)
        {
            _client = client;
            Parsers.Add(new ParserProtocolContainer(new byte[] { 0x02 }, new byte[] { 0x03 }, 1));
            BeginRead();
        }

        public DateTime CameOnline { get; } = DateTime.Now;

        public string RemoteAddress => _client.Client.RemoteEndPoint.ToString();

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

                BytesReceived(newBuffer);
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
            lock (_eventHistory)
            {
                _eventHistory.Add(DateTime.UtcNow.Ticks);
            }
            MessageCountTotal++;
            BeginSendBytes(bytes);
        }

        public int MessagesPer(uint seconds)
        {
            // Clear old data
            lock(_eventHistory)
            {
                var cutoffPoint = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(StoreHistoryForSeconds)).Ticks;
                var requestedPoint = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(seconds)).Ticks;

                _eventHistory.RemoveAll((x) => x <= cutoffPoint);

               return _eventHistory.FindAll((x) => x >= requestedPoint).Count;
            }
        }

        private void BeginSendBytes(byte[] data)
        {
            if (!_client.Connected)
            {
                Disconnect();
                return;
            }

            _dataToSendQueue.Enqueue(data);

            SendNextPacket();
        }

        private bool _isSending = false;

        private void SendNextPacket()
        {
            if (_isSending) return;

            // dequeue the next chunk
            if (!_dataToSendQueue.TryDequeue(out var data)) return;

            _isSending = true;

            try
            {
                _client.GetStream().BeginWrite(data, 0, data.Length, EndBeginSendBytes, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not send data to remote client. Error: " + ex.Message);
                Disconnect();
            }
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
            }
            finally
            {
                _isSending = false;
                SendNextPacket();
            }
        }


        private void Disconnect()
        {
            if (_client.Connected)
                _client.Close();
            InvokeOnConnectionClosed();

            // Dequeue the whole array on disconnect.
            while (_dataToSendQueue.TryDequeue(out var data)) ;

            _isSending = false;
        }


        public override async void ProcessPacket(byte[] bytes, ParserProtocolContainer parser)
        {
            // get the message
            var message = ProtocolHelper.UnWrap(bytes);
            if (message == null) return;

            if (message.MessageEvent == MessageTypeIdEnum.Unknown) return;

            if (message.MessageEvent == MessageTypeIdEnum.DeviceId)
            {
                if (message.MessageParts.ContainsKey(ProtocolMessage.DEVID))
                {
                    int id;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.DEVID], out id);
                    if (!parsed) return;


                    // Update the pins in the database
                    using (var dc = new EfHolidayContext(Program.ConnectionString))
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
                    int pinsAvail;
                    var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINSAVAIL], out pinsAvail);
                    if (!parsed) return;

                    List<string> names = new List<string>();
                    for (var i = 1; i <= pinsAvail; i++)
                    {
                        names.Add("PIN" + i);
                    }

                    if (message.MessageParts.ContainsKey(ProtocolMessage.PINNAMES))
                    {
                        var parts = message.MessageParts[ProtocolMessage.PINNAMES].Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < Math.Min(names.Count, parts.Length); i++)
                        {
                            var name = parts[i];
                            names[i] = name;
                        }
                    }

                    // Update the pins in the database
                    using (var dc = new EfHolidayContext(Program.ConnectionString))
                    {
                        var device = dc.Devices.FirstOrDefault(x => x.DeviceId == DeviceId);
                        if (device == null) return;

                        var ports = device.DeviceIoPorts.ToList();

                        for (int i = 1; i <= pinsAvail; i++)
                        {
                            var port = ports.FirstOrDefault(x => x.CommandPin == i);
                            if (port == null)
                            {
                                port = new DeviceIoPorts {DeviceId = DeviceId, CommandPin = i, Description = names[i-1]};
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
                    using (var dc = new EfHolidayContext(Program.ConnectionString))
                    {
                        var basePathSetting = await dc.Settings.Where(x=> x.SettingName == SettingKeys.FileBasePath).FirstOrDefaultAsync();
                        if (basePathSetting == null || string.IsNullOrWhiteSpace(basePathSetting.ValueString))
                        {
                            Program.LogMessage($"System Setting {SettingKeys.FileBasePath} does not exist in settings table. Must be set to support file transfers");
                            BeginSend(new ProtocolMessage(MessageTypeIdEnum.RequestFailed));
                            return;
                        }

                        if (!Directory.Exists(basePathSetting.ValueString))
                        {
                            Program.LogMessage($"Path '{basePathSetting.ValueString}' does not exist, cant send file!");
                            BeginSend(new ProtocolMessage(MessageTypeIdEnum.RequestFailed));
                            return;
                        }

                        var fileRequested = message.MessageParts[ProtocolMessage.FILEDOWNLOAD];

                        Program.LogMessage($"File Requested {fileRequested}");

                        var fileRequestedModified = fileRequested.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                        Program.LogMessage($"Modified File Requested {fileRequestedModified}");

                        // Modify the path separators based on if the server is in linux or windows.
                        var combinedPath = Path.Combine(basePathSetting.ValueString, fileRequestedModified);

                        // See if the requested file exists
                        if (!File.Exists(combinedPath))
                        {
                            Program.LogMessage($"File Requested does not exist at path {combinedPath}");
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
            Console.WriteLine("Connction Closed.");
            EventHandler handler = OnConnectionClosed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public int DeviceId { get; private set; } = -1;
    }
}
