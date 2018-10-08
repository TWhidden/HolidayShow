using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowLib;

namespace HolidayShowEndpointUniversalApp.Containers
{
    public abstract class ProtocolClient : ByteParserBase
    {
        private readonly IServerDetails _endPoint;
        private TcpClient _client;

        private NetworkStream _stream;
        private CancellationTokenSource _cancellationTokenSource;

        private const int BufferLength = 500000;

        private readonly byte[] _readBuffer = new byte[BufferLength];

        protected ProtocolClient(IServerDetails endPoint)
        {
            _endPoint = endPoint;
            Parsers.Add(new ParserProtocolContainer(new byte[] { 0x02 }, new byte[] { 0x03 }, 1));

            CreateClientSocket();
        }

        private void CreateClientSocket()
        {
            _client?.Dispose();

            _client = new TcpClient();
            _client.BeginConnect(_endPoint.EndPoint.Host, _endPoint.EndPoint.Port, ClientConnectionCompleted, null);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1000);
        }

        private void ClientConnectionCompleted(IAsyncResult r)
        {
            try
            {
                _client.EndConnect(r);

                if (!_client.Connected)
                {
                    Disconnect();
                    Console.WriteLine("Connected == False!");
                    return;
                }

                Console.WriteLine($"Connected to {_endPoint.EndPoint.Host}:{_endPoint.EndPoint.Port}!");

                _stream = _client.GetStream();

                NetworkStreamRead();

                // let the implemented class know the connection is ready
                NewConnectionEstablished();
            }
            catch (Exception)
            {
                //Console.WriteLine("Could not connect.. trying again...");
                Disconnect();
            }
        }

        private async void NetworkStreamRead()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var bufferRead = await _stream.ReadAsync(_readBuffer, 0, BufferLength, _cancellationTokenSource.Token);

                if (bufferRead == 0)
                {
                    // 0 byte indicates a disconnected.
                    Console.Write("0 bytes detected on receive. Reconnect");
                    Disconnect();
                }

                var data = new byte[bufferRead];
                Buffer.BlockCopy(_readBuffer, 0, data, 0, bufferRead);
                BytesReceived(data);
            }
            catch (Exception ex)
            {
                ErrorDetected(ex);
                Disconnect();
            }
            finally
            {
                // Requeue if the client is connected
                if (_client != null && _client.Connected)
                    NetworkStreamRead();
            }
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

                // Attempt to pull from the queue and send out the pipe.
                // if no data is avaialble, exit the function
                if (!_outDataQueue.TryDequeue(out var data))
                {
                    return;
                }

                // send the data out the pipe.
                await _stream.WriteAsync(data, 0, data.Length);

                // pull the next item out of the queue for processing if one exists.
                goto next;
            }
            catch (Exception ex)
            {
                ErrorDetected(ex);
                Disconnect();
            }
            finally
            {
                _isSending = false;
            }
        }

        public async void Disconnect(bool recreate = true)
        {
            Console.WriteLine($"Disconnect!!! Recreate? {recreate}");

            if (_client != null && _client.Connected)
            {
                _client.Client?.Shutdown(SocketShutdown.Both);
                _client.Dispose();
                _client = null;
            }

            if (!recreate) return;

            await Task.Delay(1000);
            CreateClientSocket();
        }

        public override void ProcessPacket(byte[] bytes, ParserProtocolContainer parser)
        {
            // get the message
            var message = ProtocolHelper.UnWrap(bytes);
            if (message == null) return;

            if (message.MessageEvent == MessageTypeIdEnum.Unknown) return;

            if (message.MessageEvent == MessageTypeIdEnum.KeepAlive)
            {
                var msg = new ProtocolMessage(MessageTypeIdEnum.KeepAlive);
                BeginSend(msg);
            }
            else if (message.MessageEvent == MessageTypeIdEnum.Reset)
            {
                ResetReceived();
            }
            else if (message.MessageEvent == MessageTypeIdEnum.EventControl)
            {
                EventControlReceived(message);
            }
            else if (message.MessageEvent == MessageTypeIdEnum.RequestFile)
            {
                FileRequestReceived(message);
            }
        }

        public abstract void FileRequestReceived(ProtocolMessage message);

        protected abstract void NewConnectionEstablished();

        protected abstract void ErrorDetected(Exception ex);

        protected abstract void ResetReceived();

        protected abstract void EventControlReceived(ProtocolMessage message);

    }


}
