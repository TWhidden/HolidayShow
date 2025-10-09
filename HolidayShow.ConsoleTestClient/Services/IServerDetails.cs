using System.Net;

namespace HolidayShow.ConsoleTestClient.Services
{
    public interface IServerDetails
    {
        DnsEndPoint EndPoint { get; }
    }
}