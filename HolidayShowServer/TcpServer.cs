using System;
using System.Net;
using System.Net.Sockets;

namespace HolidayShowServer
{
    public class TcpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private bool _started;

        public TcpServer(ushort listenPort)
        {
            _listener = new TcpListener(IPAddress.Any, listenPort);
        }

        public void Start()
        {
            _started = true;
            try
            {
                _listener.Start();
                AcceptClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not bind to port. In use? " + ex.Message);
            }
        }

        public event EventHandler<NewClientEventArgs> OnClientConnected;

        protected virtual void InvokeOnClientConnected(NewClientEventArgs e)
        {
            EventHandler<NewClientEventArgs> handler = OnClientConnected;
            handler?.Invoke(this, e);
        }

        private void AcceptClient()
        {
            _listener.BeginAcceptTcpClient(EndAcceptClient, null);
        }

        private void EndAcceptClient(IAsyncResult a)
        {
            try
            {
                var client = _listener.EndAcceptTcpClient(a);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1000);
                InvokeOnClientConnected(new NewClientEventArgs(client));
            }
            catch
            {
                // Ignore
            }
            finally
            {
                if (_started)
                    AcceptClient();
            }

        }

        public void Stop()
        {
            _started = false;
            _listener.Stop();
        }

        public void Dispose()
        {

        }
    }

    public class NewClientEventArgs : EventArgs
    {
        public NewClientEventArgs(TcpClient client)
        {
            Client = client;
        }

        public TcpClient Client { get; private set; }
    }
}
