using Microsoft.AspNetCore.SignalR;

namespace HolidayShowServer.Services;

public class DeviceStatusHub : Hub
{
    private readonly DeviceStatusService _deviceStatusService;

    public DeviceStatusHub(DeviceStatusService deviceStatusService)
    {
        _deviceStatusService = deviceStatusService;
    }

    public override async Task OnConnectedAsync()
    {
        // Send current device statuses to the newly connected client
        var currentStatuses = _deviceStatusService.GetAllDeviceStatuses();
        await Clients.Caller.SendAsync("DeviceStatusUpdate", currentStatuses);

        await base.OnConnectedAsync();
    }

    // This hub will handle device status updates and can be extended for other real-time features
    // Clients can subscribe to device status updates via this hub
}