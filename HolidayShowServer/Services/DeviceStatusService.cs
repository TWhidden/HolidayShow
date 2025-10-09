using System.Collections.Concurrent;
using HolidayShow.Data.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Services;

public class DeviceStatusService
{
    private readonly ConcurrentDictionary<int, DeviceStatusInfo> _deviceStatuses = new();
    private readonly IHubContext<DeviceStatusHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    public DeviceStatusService(IHubContext<DeviceStatusHub> hubContext, IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public void UpdateDeviceConnected(RemoteClient client)
    {
        var status = new DeviceStatusInfo
        {
            DeviceId = client.DeviceId,
            DeviceName = GetDeviceName(client.DeviceId),
            IpAddress = client.RemoteAddress.Split(':')[0], // Extract IP from address:port
            IsConnected = true,
            ConnectedAt = client.CameOnline,
            InstructionsSent = client.MessageCountTotal
        };

        _deviceStatuses[client.DeviceId] = status;

        // Notify all connected clients
        _hubContext.Clients.All.SendAsync("DeviceStatusUpdate", status);
    }

    public void UpdateDeviceDisconnected(RemoteClient client)
    {
        if (_deviceStatuses.TryGetValue(client.DeviceId, out var status))
        {
            status.IsConnected = false;
            status.DisconnectedAt = DateTime.Now;
            status.InstructionsSent = client.MessageCountTotal;

            // Notify all connected clients
            _hubContext.Clients.All.SendAsync("DeviceStatusUpdate", status);
        }
    }

    public void UpdateDeviceStats(RemoteClient client)
    {
        if (_deviceStatuses.TryGetValue(client.DeviceId, out var status))
        {
            status.InstructionsSent = client.MessageCountTotal;
            // Could send periodic updates, but for now we'll update on connect/disconnect
        }
    }

    public IEnumerable<DeviceStatusInfo> GetAllDeviceStatuses()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
        
        // Get all devices from the database
        var allDevices = context.Devices.ToList();
        
        var allStatuses = new List<DeviceStatusInfo>();
        
        foreach (var device in allDevices)
        {
            if (_deviceStatuses.TryGetValue(device.DeviceId, out var currentStatus))
            {
                // Device is currently tracked (connected or recently disconnected)
                allStatuses.Add(currentStatus);
            }
            else
            {
                // Device exists in database but not currently tracked
                allStatuses.Add(new DeviceStatusInfo
                {
                    DeviceId = device.DeviceId,
                    DeviceName = device.Name ?? $"Device {device.DeviceId}",
                    IpAddress = string.Empty,
                    IsConnected = false,
                    ConnectedAt = null,
                    DisconnectedAt = null,
                    InstructionsSent = 0
                });
            }
        }
        
        return allStatuses;
    }

    private string GetDeviceName(int deviceId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
        
        var device = context.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        return device?.Name ?? $"Device {deviceId}";
    }
}

public class DeviceStatusInfo
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public long InstructionsSent { get; set; }
}