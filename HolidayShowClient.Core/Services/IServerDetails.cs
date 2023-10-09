using System.Net;

namespace HolidayShowClient.Core.Services
{
    public interface IServerDetails
    {
        DnsEndPoint EndPoint { get; }
    }
}