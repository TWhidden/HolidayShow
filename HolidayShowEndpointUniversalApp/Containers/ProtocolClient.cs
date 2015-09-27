using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowLib;

namespace HolidayShowEndpointUniversalApp.Containers
{
    public abstract class ProtocolClient : ByteParserBase 
    {
        private Socket _client;
        private readonly SocketAsyncEventArgs _socketConnectArgs;
        private readonly SocketAsyncEventArgs _socketReceiveArgs;
        private readonly SocketAsyncEventArgs _socketSendArgs;
        private const int BufferLength = 500000;

        protected ProtocolClient(IServerDetails endPoint)
        {
            CreateClientSocket();
            Parsers.Add(new ParserProtocolContainer(new byte[]{0x02}, new byte[]{0x03}, 1));

            _socketConnectArgs = new SocketAsyncEventArgs {RemoteEndPoint = endPoint.EndPoint };
            _socketConnectArgs.Completed += SocketConnectionCompleted;

            _socketReceiveArgs = new SocketAsyncEventArgs();
            _socketReceiveArgs.Completed += SocketReceiveCompleted;
            var receiveBuffer = new byte[BufferLength];
            _socketReceiveArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

            _socketSendArgs = new SocketAsyncEventArgs();
            _socketSendArgs.Completed += SocketSendCompleted;

            CreateConnection();

        }

        private void CreateClientSocket()
        {
            _client?.Dispose();

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }


        private void CreateConnection()
        {
            _client.ConnectAsync(_socketConnectArgs);
        }

        private void SocketSendCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        private async void SocketConnectionCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (!_client.Connected)
                {
                    Disconnect();
                    return;
                }

                // Register the async receive callback
                _client.ReceiveAsync(_socketReceiveArgs);

                // let the implemented class know the connection is ready
                NewConnectionEstablished();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Could not connect.. trying again...");
                await Task.Delay(1000);
                Disconnect();
            }
        }

        private void SocketReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred == 0)
                {
                    Disconnect();
                    return;
                }

                Debug.WriteLine("Bytes Received from server {0}", e.BytesTransferred);
                
                // take the bytes transfered from the array
                byte[] data;
                if (e.BytesTransferred == BufferLength)
                {
                    data = e.Buffer;
                }
                else
                {
                    data = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesTransferred);
                }

                // Dump into the base class looking for the protocol
                BytesReceived(data);
            }
            catch (Exception ex)
            {
                ErrorDetected(ex);
                Disconnect();
                return;
            }
            _client.ReceiveAsync(_socketReceiveArgs);
        }

        protected void BeginSend(ProtocolMessage message)
        {
            var bytes = ProtocolHelper.Wrap(message);
            BeginSendBytes(bytes);
        }

        private void BeginSendBytes(byte[] data)
        {
            _socketSendArgs.SetBuffer(data, 0, data.Length);
            _client.SendAsync(_socketSendArgs);
        }

        public void Disconnect(bool recreate = true)
        {
            if(_client.Connected)
                _client.Shutdown(SocketShutdown.Both);

            if(recreate)
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
            }else if (message.MessageEvent == MessageTypeIdEnum.Reset)
            {
                ResetReceived();
            }
            else if (message.MessageEvent == MessageTypeIdEnum.EventControl)
            {
                EventControlReceived(message);
            }else if (message.MessageEvent == MessageTypeIdEnum.RequestFile)
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
