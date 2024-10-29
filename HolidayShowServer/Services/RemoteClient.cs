using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using HolidayShow.Data.Core;
using HolidayShowLib;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Services;

public class RemoteClient
{
    private readonly byte[] _buffer;
    private readonly int _bufferSize = 2048; // Adjust as needed
    private readonly TcpClient _client;
    private readonly ILogger<RemoteClient> _logger;
    private readonly ConcurrentQueue<ProtocolMessage> _sendQueue = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly NetworkStream _stream;
    private bool _isConnected = true;
    private bool _isSending;

    public RemoteClient(TcpClient client, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = _client.GetStream();
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = loggerFactory.CreateLogger<RemoteClient>();
        _buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
    }

    public DateTime CameOnline { get; } = DateTime.Now;
    public string RemoteAddress => _client.Client.RemoteEndPoint?.ToString() ?? "N/A";
    public int DeviceId { get; private set; } = -1;
    public long MessageCountTotal { get; private set; }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isConnected && !cancellationToken.IsCancellationRequested)
            {
                var bytesRead = 0;

                try
                {
                    bytesRead = await _stream.ReadAsync(_buffer, 0, _bufferSize, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, $"IO Exception while reading from client {RemoteAddress}");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error while reading from client {RemoteAddress}");
                    break;
                }

                if (bytesRead == 0)
                {
                    _logger.LogInformation($"Client {RemoteAddress} disconnected gracefully.");
                    break;
                }

                // Unwrap the message before handling
                var message = ProtocolHelper.UnWrap(_buffer, bytesRead);
                if (message != null)
                    await HandleReceivedMessage(message).ConfigureAwait(false);
                else
                    _logger.LogWarning($"Received malformed message from {RemoteAddress}");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"Processing for client {RemoteAddress} was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing client {RemoteAddress}");
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task HandleReceivedMessage(ProtocolMessage message)
    {
        switch (message.MessageEvent)
        {
            case MessageTypeIdEnum.DeviceId:
                await HandleDeviceIdMessageAsync(message).ConfigureAwait(false);
                break;

            case MessageTypeIdEnum.RequestFile:
                await HandleRequestFileMessageAsync(message).ConfigureAwait(false);
                break;

            // Handle other message types as needed
            default:
                _logger.LogDebug($"Unhandled message type {message.MessageEvent} from {RemoteAddress}");
                break;
        }
    }

    private async Task HandleDeviceIdMessageAsync(ProtocolMessage message)
    {
        if (message.MessageParts.TryGetValue(ProtocolMessage.DEVID, out var devIdStr) &&
            int.TryParse(devIdStr, out var devId))
        {
            DeviceId = devId;
            _logger.LogInformation($"Device ID {DeviceId} received from {RemoteAddress}");

            await UpdateDeviceAsync(DeviceId).ConfigureAwait(false);

            if (message.MessageParts.TryGetValue(ProtocolMessage.PINSAVAIL, out var pinsAvailStr) &&
                int.TryParse(pinsAvailStr, out var pinsAvail))
            {
                var pinNames = Enumerable.Range(1, pinsAvail)
                    .Select(i => $"PIN{i}")
                    .ToList();

                if (message.MessageParts.TryGetValue(ProtocolMessage.PINNAMES, out var pinNamesStr))
                {
                    var providedPinNames = pinNamesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < Math.Min(pinNames.Count, providedPinNames.Length); i++)
                        pinNames[i] = providedPinNames[i];
                }

                await UpdatePinsAsync(DeviceId, pinsAvail, pinNames).ConfigureAwait(false);
            }
        }
        else
        {
            _logger.LogWarning($"Invalid Device ID message from {RemoteAddress}");
        }
    }

    private async Task HandleRequestFileMessageAsync(ProtocolMessage message)
    {
        if (message.MessageParts.TryGetValue(ProtocolMessage.FILEDOWNLOAD, out var fileRequested))
        {
            _logger.LogInformation($"File request for '{fileRequested}' from {RemoteAddress}");
            await SendFileAsync(fileRequested).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning($"FileDownload key missing in RequestFile message from {RemoteAddress}");
            await SendMessageAsync(new ProtocolMessage(MessageTypeIdEnum.RequestFailed)).ConfigureAwait(false);
        }
    }

    private async Task UpdateDeviceAsync(int deviceId)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
            var device = await db.Devices.FirstOrDefaultAsync(x => x.DeviceId == deviceId).ConfigureAwait(false);
            if (device == null)
            {
                device = new Devices { DeviceId = deviceId, Name = "New Device" };
                db.Devices.Add(device);
                await db.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation($"New device with ID {deviceId} added to the database.");
            }
            else
            {
                _logger.LogDebug($"Device with ID {deviceId} already exists in the database.");
            }
        }
    }

    private async Task UpdatePinsAsync(int deviceId, int pinsAvail, List<string> pinNames)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
            var device = await db.Devices
                .Include(d => d.DeviceIoPorts)
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId)
                .ConfigureAwait(false);
            if (device == null)
            {
                _logger.LogWarning($"Attempted to update pins for non-existent device ID {deviceId}");
                return;
            }

            for (var i = 1; i <= pinsAvail; i++)
            {
                var port = device.DeviceIoPorts.FirstOrDefault(x => x.CommandPin == i);
                if (port == null)
                {
                    port = new DeviceIoPorts { DeviceId = deviceId, CommandPin = i, Description = pinNames[i - 1] };
                    db.DeviceIoPorts.Add(port);
                    _logger.LogInformation(
                        $"Added CommandPin {i} with description '{pinNames[i - 1]}' to device ID {deviceId}");
                }
            }

            // Ensure a default port exists
            if (device.DeviceIoPorts.All(x => x.CommandPin != -1))
            {
                db.DeviceIoPorts.Add(new DeviceIoPorts
                    { DeviceId = deviceId, CommandPin = -1, Description = "NONE", IsNotVisable = true });
                _logger.LogInformation($"Added default CommandPin -1 to device ID {deviceId}");
            }

            await db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug($"Pins updated for device ID {deviceId}");
        }
    }

    private async Task SendFileAsync(string fileRequested)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
            var basePathSetting = await db.Settings
                .FirstOrDefaultAsync(x => x.SettingName == SettingKeys.FileBasePath)
                .ConfigureAwait(false);

            if (basePathSetting == null || string.IsNullOrWhiteSpace(basePathSetting.ValueString))
            {
                _logger.LogWarning($"System Setting '{SettingKeys.FileBasePath}' is missing or empty.");
                await SendMessageAsync(new ProtocolMessage(MessageTypeIdEnum.RequestFailed)).ConfigureAwait(false);
                return;
            }

            var basePath = basePathSetting.ValueString;
            var sanitizedPath = fileRequested.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(basePath, sanitizedPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning($"Requested file '{fullPath}' does not exist.");
                await SendMessageAsync(new ProtocolMessage(MessageTypeIdEnum.RequestFailed)).ConfigureAwait(false);
                return;
            }

            try
            {
                var fileBytes = await File.ReadAllBytesAsync(fullPath).ConfigureAwait(false);
                var responseMessage = new ProtocolMessage(MessageTypeIdEnum.RequestFile, new Dictionary<string, string>
                    {
                        { ProtocolMessage.AUDIOFILE, fileRequested },
                        { ProtocolMessage.FILEBYTES, Convert.ToBase64String(fileBytes) }
                    }
                );

                await SendMessageAsync(responseMessage).ConfigureAwait(false);
                _logger.LogInformation($"Sent file '{fileRequested}' to {RemoteAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending file '{fullPath}' to {RemoteAddress}");
                await SendMessageAsync(new ProtocolMessage(MessageTypeIdEnum.RequestFailed)).ConfigureAwait(false);
            }
        }
    }

    public async Task SendMessageAsync(ProtocolMessage message)
    {
        _sendQueue.Enqueue(message);
        await SendNextAsync().ConfigureAwait(false);
    }

    private async Task SendNextAsync()
    {
        if (_isSending) return;
        if (!_sendQueue.TryDequeue(out var message)) return;

        try
        {
            _isSending = true;
            var data = ProtocolHelper.Wrap(message);
            await _stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            MessageCountTotal++;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, $"IO Exception while sending data to {RemoteAddress}");
            Disconnect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error while sending data to {RemoteAddress}");
            Disconnect();
        }
        finally
        {
            _isSending = false;
            await SendNextAsync().ConfigureAwait(false);
        }
    }

    private void Disconnect()
    {
        if (_isConnected)
        {
            _isConnected = false;
            _client.Close();
            ArrayPool<byte>.Shared.Return(_buffer);
            _logger.LogInformation($"Disconnected client {RemoteAddress}");
        }
    }
}