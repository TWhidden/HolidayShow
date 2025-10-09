using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HolidayShow.ConsoleTestClient.Services;
using HolidayShowLib;

namespace HolidayShow.ConsoleTestClient.Containers
{
    public class TestClient : ByteParserBase
    {
        private readonly IServerDetails _endPoint;
        private readonly int _deviceId;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cancellationTokenSource;
        private const int BufferLength = 500000;
        private readonly byte[] _readBuffer = new byte[BufferLength];

        public TestClient(IServerDetails endPoint, int deviceId)
        {
            _endPoint = endPoint;
            _deviceId = deviceId;
            Parsers.Add(new ParserProtocolContainer(new byte[] { 0x02 }, new byte[] { 0x03 }, 1));
            CreateClientSocket();
        }

        private void CreateClientSocket()
        {
            _client?.Dispose();
            _client = new TcpClient();

            try
            {
                _client.Connect(_endPoint.EndPoint.Host, _endPoint.EndPoint.Port);
                Program._clientConnected = true;
                Console.WriteLine($"✅ Connected to {_endPoint.EndPoint.Host}:{_endPoint.EndPoint.Port}!");

                _stream = _client.GetStream();
                NetworkStreamRead();

                // Register the device
                RegisterDevice();
            }
            catch (Exception ex)
            {
                Program._clientConnected = false;
                Console.WriteLine($"❌ Failed to connect: {ex.Message}");
                // Retry connection after delay
                Task.Delay(2000).ContinueWith(_ => CreateClientSocket());
            }
        }

        private async void NetworkStreamRead()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var bufferRead = await _stream!.ReadAsync(_readBuffer, 0, BufferLength, _cancellationTokenSource!.Token);

                if (bufferRead == 0)
                {
                    Console.WriteLine("🔌 Disconnected (0 bytes received)");
                    Disconnect();
                    return;
                }

                var data = new byte[bufferRead];
                Buffer.BlockCopy(_readBuffer, 0, data, 0, bufferRead);
                BytesReceived(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading from stream: {ex.Message}");
                Disconnect();
            }
            finally
            {
                // Requeue if the client is connected
                if (_client != null && _client.Connected)
                    NetworkStreamRead();
            }
        }

        private void RegisterDevice()
        {
            var dic = new Dictionary<string, string>
            {
                {ProtocolMessage.DEVID, _deviceId.ToString()},
                {ProtocolMessage.PINSAVAIL, "8"}, // Fake 8 pins
                {ProtocolMessage.PINNAMES, "TestPin1,TestPin2,TestPin3,TestPin4,TestPin5,TestPin6,TestPin7,TestPin8"}
            };

            var message = new ProtocolMessage(MessageTypeIdEnum.DeviceId, dic);
            BeginSend(message);

            Console.WriteLine($"📝 Registered device {_deviceId} with 8 test pins");
        }

        protected void BeginSend(ProtocolMessage message)
        {
            var bytes = ProtocolHelper.Wrap(message);
            _outDataQueue.Enqueue(bytes);
            BeginSendImpl();
        }

        private bool _isSending = false;
        private readonly ConcurrentQueue<byte[]> _outDataQueue = new ConcurrentQueue<byte[]>();

        private async void BeginSendImpl()
        {
            if (_isSending) return;

            try
            {
                _isSending = true;

                next:

                if (!_outDataQueue.TryDequeue(out var data))
                {
                    return;
                }

                await _stream!.WriteAsync(data, 0, data.Length);
                goto next;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending data: {ex.Message}");
                Disconnect();
            }
            finally
            {
                _isSending = false;
            }
        }

        public void Disconnect(bool recreate = true)
        {
            Console.WriteLine("🔌 Disconnecting...");

            if (_client != null && _client.Connected)
            {
                _client.Client?.Shutdown(SocketShutdown.Both);
                _client.Dispose();
                _client = null;
            }

            if (!recreate) return;

            // Reconnect after delay
            Task.Delay(5000).ContinueWith(_ => CreateClientSocket());
        }

        public override void ProcessPacket(byte[] bytes, ParserProtocolContainer parser)
        {
            var message = ProtocolHelper.UnWrap(bytes);
            if (message == null) return;

            switch (message.MessageEvent)
            {
                case MessageTypeIdEnum.KeepAlive:
                    // Respond to keep-alive
                    var keepAliveMsg = new ProtocolMessage(MessageTypeIdEnum.KeepAlive);
                    BeginSend(keepAliveMsg);
                    LogMessage("💓 Keep-Alive received and responded", ConsoleColor.Green);
                    break;

                case MessageTypeIdEnum.Reset:
                    LogMessage("🔄 Reset received - All pins off", ConsoleColor.Yellow);
                    break;

                case MessageTypeIdEnum.EventControl:
                    HandleEventControl(message);
                    break;

                case MessageTypeIdEnum.RequestFile:
                    LogMessage($"📁 File request: {string.Join(", ", message.MessageParts)}", ConsoleColor.Cyan);
                    break;

                default:
                    LogMessage($"📨 Unknown message type: {message.MessageEvent}", ConsoleColor.Gray);
                    break;
            }
        }

        private void HandleEventControl(ProtocolMessage message)
        {
            var parts = string.Join(", ", message.MessageParts.Select(kv => $"{kv.Key}={kv.Value}"));

            if (message.MessageParts.ContainsKey(ProtocolMessage.PINID) &&
                message.MessageParts.ContainsKey(ProtocolMessage.DURATION) &&
                message.MessageParts.ContainsKey(ProtocolMessage.PINON))
            {
                var pinId = message.MessageParts[ProtocolMessage.PINID];
                var duration = message.MessageParts[ProtocolMessage.DURATION];
                var pinOn = message.MessageParts[ProtocolMessage.PINON];

                var action = pinOn == "1" ? "ON" : "OFF";
                var color = pinOn == "1" ? ConsoleColor.Green : ConsoleColor.Red;

                if (duration == "0")
                {
                    LogMessage($"💡 Pin {pinId} turned {action}", color);
                }
                else
                {
                    LogMessage($"⏱️  Pin {pinId} turned {action} for {duration}ms", color);
                }
            }
            else if (message.MessageParts.ContainsKey(ProtocolMessage.AUDIOFILE))
            {
                var audioFile = message.MessageParts[ProtocolMessage.AUDIOFILE];
                LogMessage($"🎵 Audio file requested: {audioFile}", ConsoleColor.Magenta);
            }
            else
            {
                LogMessage($"🎮 Event control: {parts}", ConsoleColor.Blue);
            }
        }

        private void LogMessage(string message, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ForegroundColor = originalColor;
        }
    }
}