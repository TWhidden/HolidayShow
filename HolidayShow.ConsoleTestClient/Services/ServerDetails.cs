using System.Net;

namespace HolidayShow.ConsoleTestClient.Services
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