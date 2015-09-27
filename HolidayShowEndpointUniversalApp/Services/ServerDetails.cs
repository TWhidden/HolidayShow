using System.Net;

namespace HolidayShowEndpointUniversalApp.Services
{
    public class ServerDetails : IServerDetails
    {
        public ServerDetails(DnsEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public DnsEndPoint EndPoint { get; }
    }
}
