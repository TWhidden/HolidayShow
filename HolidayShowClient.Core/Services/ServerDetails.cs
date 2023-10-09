using System.Net;

namespace HolidayShowClient.Core.Services
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
