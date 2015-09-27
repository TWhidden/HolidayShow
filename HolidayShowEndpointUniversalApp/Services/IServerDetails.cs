using System.Net;

namespace HolidayShowEndpointUniversalApp.Services
{
    public interface IServerDetails
    {
        DnsEndPoint EndPoint { get; }
    }
}