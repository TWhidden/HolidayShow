using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using HolidayShow.Data.Core;
using HolidayShowLib;
using HolidayShowLib.Core;
using Microsoft.EntityFrameworkCore;

namespace HolidayShowServer.Services;

public class TcpBackgroundService : BackgroundService
{
    private readonly ConcurrentDictionary<TcpClient, RemoteClient> _clients = new();
    private readonly IConfiguration _configuration;
    private readonly TcpListener _listener;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentQueue<string> _logMessages = new();
    private readonly ConcurrentDictionary<Timer, bool> _queuedTimers = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly int _port;
    private bool _running = true;
    private bool _setExecuting;

    public TcpBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger("TcpServer");
        _port = _configuration.GetValue<int>("ServerSettings:Port");
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener.Start();
        _logger.LogInformation($"TCP Server started on port {_port}");

        // Start client acceptance loop
        var acceptClientsTask = AcceptClientsAsync(stoppingToken);

        // Start server processing loop
        var runServerTask = RunServerAsync(stoppingToken);

        // Wait for both tasks to complete
        await Task.WhenAll(acceptClientsTask, runServerTask);
    }

    private async Task AcceptClientsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                var remoteClient = new RemoteClient(tcpClient, _serviceProvider, _loggerFactory);

                if (_clients.TryAdd(tcpClient, remoteClient))
                {
                    // Hook for Pub/Sub: Client Connected
                    OnClientConnected(remoteClient);

                    _logger.LogInformation($"Client connected: {remoteClient.RemoteAddress}");

                    _ = remoteClient.ProcessAsync(stoppingToken).ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            _logger.LogError(t.Exception, $"Error processing client {remoteClient.RemoteAddress}");

                        _clients.TryRemove(tcpClient, out _);

                        // Hook for Pub/Sub: Client Disconnected
                        OnClientDisconnected(remoteClient);

                        _logger.LogInformation($"Client disconnected: {remoteClient.RemoteAddress}");
                    }, TaskScheduler.Current);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                // Listener has been stopped
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting client");
            }

        _listener.Stop();
    }

    private async Task RunServerAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RunServer task started.");

        while (_running && !stoppingToken.IsCancellationRequested)
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EfHolidayContext>();
                    var settingKeysOfInterest = new List<string>
                    {
                        SettingKeys.OnAt,
                        SettingKeys.OffAt,
                        SettingKeys.Refresh,
                        SettingKeys.AudioOnAt,
                        SettingKeys.AudioOffAt,
                        SettingKeys.SetPlaybackOption,
                        SettingKeys.DelayBetweenSets,
                        SettingKeys.CurrentSet,
                        SettingKeys.DetectDevicePin,
                        SettingKeys.IsAudioEnabled,
                        SettingKeys.IsDangerEnabled
                    };

                    var settings = await db.Settings
                        .Where(x => settingKeysOfInterest.Contains(x.SettingName))
                        .ToListAsync(stoppingToken)
                        .ConfigureAwait(false);

                    // Check schedule
                    var scheduleOn = settings.FirstOrDefault(x => x.SettingName == SettingKeys.OnAt);
                    var scheduleOff = settings.FirstOrDefault(x => x.SettingName == SettingKeys.OffAt);
                    if (scheduleOn != null && scheduleOff != null &&
                        !string.IsNullOrWhiteSpace(scheduleOn.ValueString))
                    {
                        var isScheduleEnabled =
                            IsCurrentTimeBetweenSettingTimes(scheduleOn.ValueString, scheduleOff.ValueString);

                        if (!isScheduleEnabled)
                        {
                            _logger.LogInformation(
                                $"Schedule is currently off. Start: '{scheduleOn.ValueString}' -> '{scheduleOff.ValueString}'; Current Time: '{DateTime.Now.TimeOfDay}'");
                            await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
                            continue;
                        }
                    }

                    var refresh = settings.FirstOrDefault(x => x.SettingName == SettingKeys.Refresh);
                    if (refresh != null)
                    {
                        db.Settings.Remove(refresh);
                        await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                        InitiateReset();
                        _setExecuting = false;
                        _logger.LogInformation("Reset called and executed");
                    }

                    if (!_setExecuting)
                    {
                        _setExecuting = true;

                        settings = await db.Settings
                            .Where(x => settingKeysOfInterest.Contains(x.SettingName))
                            .ToListAsync(stoppingToken)
                            .ConfigureAwait(false);

                        var audioOnAt = settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOnAt);
                        var audioOffAt = settings.FirstOrDefault(x => x.SettingName == SettingKeys.AudioOffAt);

                        var isAudioScheduleEnabled = true;
                        if (audioOnAt != null && audioOffAt != null &&
                            !string.IsNullOrWhiteSpace(audioOnAt.ValueString))
                            isAudioScheduleEnabled =
                                IsCurrentTimeBetweenSettingTimes(audioOnAt.ValueString, audioOffAt.ValueString);

                        var option = settings.FirstOrDefault(x => x.SettingName == SettingKeys.SetPlaybackOption);
                        if (option == null)
                        {
                            _logger.LogWarning("Set Playback option not set. Cannot start");
                            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                            _setExecuting = false;
                            continue;
                        }

                        var playbackOption = (SetPlaybackOptionEnum)option.ValueDouble;

                        var delayBetweenSets = settings.Where(x => x.SettingName == SettingKeys.DelayBetweenSets)
                            .Select(x => (int)x.ValueDouble)
                            .FirstOrDefault();
                        if (delayBetweenSets <= 0) delayBetweenSets = 5000;

                        var setId = -1;

                        switch (playbackOption)
                        {
                            case SetPlaybackOptionEnum.Off:
                            {
                                if (!_running) InitiateReset();

                                _setExecuting = false;

                                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                break;
                            }
                            case SetPlaybackOptionEnum.PlaybackRandom:
                            {
                                InitiateReset();
                                // Get all the sets, random set one.
                                var sets = await db.Sets.Where(x => !x.IsDisabled).ToListAsync(stoppingToken)
                                    .ConfigureAwait(false);
                                if (sets.Count == 0)
                                {
                                    _logger.LogWarning("No sets are configured");
                                    _setExecuting = false;
                                    await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                    break;
                                }

                                if (sets.Count == 1)
                                {
                                    _logger.LogInformation("Only one set available. Setting desired set");
                                    setId = sets[0].SetId;
                                }
                                else
                                {
                                    var random = new Random();
                                    var index = random.Next(sets.Count);
                                    setId = sets[index].SetId;
                                }
                            }
                                break;
                            case SetPlaybackOptionEnum.PlaybackCurrentOnly:
                            {
                                InitiateReset();
                                // get the set that is currently running
                                var currentSet =
                                    settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                                if (currentSet == null)
                                {
                                    _logger.LogWarning("Current set is not set.");
                                    _setExecuting = false;
                                    await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                        break;
                                }

                                var set = await db.Sets
                                    .FirstOrDefaultAsync(
                                        x => x.SetId == (int)currentSet.ValueDouble && !x.IsDisabled, stoppingToken)
                                    .ConfigureAwait(false);
                                if (set == null)
                                {
                                    _logger.LogWarning(
                                        "Current set references a set that does not exist.");

                                    await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                        _setExecuting = false;
                                    break;
                                }

                                // if we are here, this is a valid set.
                                setId = set.SetId;
                            }
                                break;
                            case SetPlaybackOptionEnum.DevicePinDetect:
                            {
                                InitiateReset();
                                var pinDetect = settings.FirstOrDefault(x =>
                                    x.SettingName == SettingKeys.DetectDevicePin);
                                if (pinDetect == null)
                                {
                                    _logger.LogWarning("No Detect Device Pin Set");
                                    _setExecuting = false;
                                    await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                    break;
                                }

                                // Get the Detect Set
                                const string SET_DETECT_NAME = "DETECTION SET";
                                var set = await db.Sets
                                    .Include(s => s.SetSequences)
                                    .FirstOrDefaultAsync(x => x.SetName == SET_DETECT_NAME, stoppingToken)
                                    .ConfigureAwait(false);
                                if (set == null)
                                {
                                    set = new Sets
                                    {
                                        SetName = SET_DETECT_NAME,
                                        IsDisabled =
                                            true // Just keeps this from showing up in the random profile. We ignore this right now.
                                    };
                                    db.Sets.Add(set);
                                    await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                                }

                                var sequence = set.SetSequences.FirstOrDefault();
                                if (sequence == null)
                                {
                                    sequence = new SetSequences { SetId = set.SetId };
                                    db.SetSequences.Add(sequence);
                                    await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                                }

                                const string EFFECT_DETECT_NAME = "DETECT EFFECT";

                                var effectDetect = await db.DeviceEffects
                                    .FirstOrDefaultAsync(x => x.EffectName == EFFECT_DETECT_NAME, stoppingToken)
                                    .ConfigureAwait(false);
                                if (effectDetect == null)
                                {
                                    effectDetect = new DeviceEffects
                                    {
                                        EffectName = EFFECT_DETECT_NAME,
                                        Duration = 2000,
                                        EffectInstructionsAvailable = await db.EffectInstructionsAvailable
                                            .FirstOrDefaultAsync(
                                                x => x.InstructionName == EffectsSupported.GPIO_STROBE,
                                                stoppingToken)
                                            .ConfigureAwait(false)
                                    };
                                    db.DeviceEffects.Add(effectDetect);
                                }

                                var metaData = $"DEVPINS={pinDetect.ValueString};DUR=500";

                                if (effectDetect.InstructionMetaData != metaData)
                                {
                                    // format: DEVPINS=1:1;DUR=50
                                    effectDetect.InstructionMetaData = metaData;
                                    sequence.DeviceEffects = effectDetect;
                                    await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                                }

                                setId = set.SetId;
                            }
                                break;
                            default:
                            {
                                _logger.LogWarning("Invalid SetPlaybackOption.");
                                _setExecuting = false;
                                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                                break;
                            }
                        }

                        if (!_setExecuting) continue;

                        // Load the set data
                        var setData = await db.Sets
                            .AsSplitQuery() // Opt-in to split query behavior
                            .Include(s => s.SetSequences)
                            .ThenInclude(ss => ss.DevicePatterns)
                            
                            .ThenInclude(dp => dp.DevicePatternSequences)
                            .ThenInclude(d => d.AudioOptions)
                            .ThenInclude(dp => dp.DevicePatternSequences)
                            .ThenInclude(d => d.DeviceIoPorts)
                            
                            .Include(s => s.SetSequences)
                            .ThenInclude(ss => ss.DeviceEffects)
                            .ThenInclude(de => de.EffectInstructionsAvailable)
                            .FirstOrDefaultAsync(x => x.SetId == setId, stoppingToken)
                            .ConfigureAwait(false);

                        if (setData == null)
                        {
                            _logger.LogWarning($"Set with ID {setId} not found.");
                            _setExecuting = false;
                            continue;
                        }

                        _logger.LogInformation($"Set: {setData.SetName}");
                        var setBuildSw = Stopwatch.StartNew();

                        // Update the current set playing
                        var current = settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                        if (current == null)
                        {
                            current = new Settings
                            {
                                SettingName = SettingKeys.CurrentSet,
                                ValueDouble = setId,
                                ValueString = string.Empty
                            };
                            db.Settings.Add(current);
                        }
                        else
                        {
                            if (current.ValueDouble != setId) current.ValueDouble = setId;
                        }

                        await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);

                        // Check if audio and danger is enabled
                        var isAudioEnabled =
                            settings.FirstOrDefault(x => x.SettingName == SettingKeys.IsAudioEnabled)
                                ?.ValueDouble == 1;
                        var isDangerEnabled =
                            settings.FirstOrDefault(x => x.SettingName == SettingKeys.IsDangerEnabled)
                                ?.ValueDouble == 1;

                        var disabledPins = new Dictionary<int, List<int>>();
                        if (!isDangerEnabled)
                            disabledPins = await db.DeviceIoPorts
                                .Where(x => x.IsDanger)
                                .GroupBy(x => x.DeviceId)
                                .ToDictionaryAsync(
                                    g => g.Key,
                                    g => g.Select(x => x.CommandPin).ToList(),
                                    stoppingToken)
                                .ConfigureAwait(false);

                        // If the schedule says it's off, disable here.
                        if (!isAudioScheduleEnabled) isAudioEnabled = false;

                        // Create the workload
                        var deviceInstructions = new List<DeviceInstructions>();

                        foreach (var setSequence in setData.SetSequences.OrderBy(x => x.OnAt))
                        {
                            var deviceId = setSequence.DevicePatterns?.DeviceId;

                            var startingOffset = setSequence.OnAt;

                            if (!deviceId.HasValue) continue;
                            foreach (var pattern in setSequence.DevicePatterns.DevicePatternSequences)
                            {
                                var onAt = startingOffset + pattern.OnAt;

                                var di = new DeviceInstructions(deviceId.Value, MessageTypeIdEnum.EventControl)
                                {
                                    OnAt = onAt
                                };

                                var set = false;
                                if (pattern.DeviceIoPorts != null
                                    && pattern.DeviceIoPorts.CommandPin >= 0
                                    && (!pattern.DeviceIoPorts.IsDanger ||
                                        (pattern.DeviceIoPorts.IsDanger && isDangerEnabled)))
                                {
                                    di.CommandPin = pattern.DeviceIoPorts.CommandPin;
                                    di.PinDuration = pattern.Duration;
                                    set = true;
                                }

                                if (!string.IsNullOrWhiteSpace(pattern.AudioOptions?.FileName))
                                {
                                    di.AudioFileName = pattern.AudioOptions.FileName;
                                    di.AudioDuration = pattern.AudioOptions.AudioDuration;
                                    set = true;
                                }

                                if (set) deviceInstructions.Add(di);
                            }
                        }

                        var durationList = new List<int> { 0 };

                        var audioTopDuration = deviceInstructions
                            .Where(x => x.AudioDuration != null)
                            .OrderByDescending(x => x.OnAt + x.AudioDuration.Value)
                            .Select(x => x.OnAt + x.AudioDuration.Value)
                            .FirstOrDefault();
                        if (audioTopDuration > 0)
                            durationList.Add(audioTopDuration);

                        _logger.LogInformation(audioTopDuration > 0
                            ? $"Top Audio Duration: {audioTopDuration}"
                            : "No Audio In set");

                        var pinTopDuration = deviceInstructions
                            .OrderByDescending(x => x.OnAt + x.PinDuration.GetValueOrDefault())
                            .Select(x => x.OnAt + x.PinDuration.GetValueOrDefault())
                            .FirstOrDefault();

                        _logger.LogInformation(pinTopDuration > 0
                            ? $"Top Pin Duration: {pinTopDuration}"
                            : "No Pins in set");

                        if (pinTopDuration > 0)
                            durationList.Add(pinTopDuration);

                        var totalSetTimeLength = durationList.Max();

                        if (totalSetTimeLength == 0)
                            // It may be that just effects are listed, and they have have a desired runtime, so we will 
                            // alter the time to go off the highest time in the set.
                            totalSetTimeLength = setData.SetSequences
                                .Where(x => x.DeviceEffects != null)
                                .OrderByDescending(x => x.OnAt + x.DeviceEffects.Duration)
                                .Select(x => x.OnAt + x.DeviceEffects.Duration)
                                .FirstOrDefault();

                        foreach (var setSequence in setData.SetSequences.Where(x => x.DeviceEffects != null)
                                     .OrderBy(x => x.OnAt))
                        {

                            // New Feature added 2015 - Effects
                            // This is something more global, that will be controlled at the server side
                            // They are hard-coded effects. at this userTime, baked into the solution. Later, there may be
                            // a plug-able architecture but for the sake of userTime, this is all hard coded.
                            if (setSequence.DeviceEffects == null ||
                                setSequence.DeviceEffects.EffectInstructionsAvailable.IsDisabled) continue;

                            var timeOn = setSequence.DeviceEffects.TimeOn;
                            var timeOff = setSequence.DeviceEffects.TimeOff;

                            if (!string.IsNullOrWhiteSpace(timeOn) && !string.IsNullOrWhiteSpace(timeOff))
                                if (!IsCurrentTimeBetweenSettingTimes(timeOn, timeOff))
                                    continue;

                            switch (setSequence.DeviceEffects.EffectInstructionsAvailable.InstructionName)
                            {
                                case EffectsSupported.GPIO_RANDOM:
                                {
                                    var data = EffectRandom(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                                case EffectsSupported.GPIO_STROBE:
                                {
                                    var data = EffectStrobe(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                                case EffectsSupported.GPIO_STAY_ON:
                                {
                                    var data = EffectStayOn(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                                case EffectsSupported.GPIO_STROBE_DELAY:
                                {
                                    var data = EffectStrobeDelay(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                                case EffectsSupported.GPIO_SEQUENTIAL:
                                {
                                    var data = EffectSequential(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                                case EffectsSupported.GPIO_RANDOM_DELAY:
                                {
                                    var data = EffectRandomDelay(setSequence, totalSetTimeLength, disabledPins);
                                    if (data != null) deviceInstructions.AddRange(data);
                                }
                                    break;
                            }
                        }

                        // #9 - Now that audio and effects have been added, we need to check to see
                        // if audio is enabled, and if its not, remove any audio.
                        if (!isAudioEnabled)
                            foreach (var audioInstruction in deviceInstructions.Where(x => x.AudioDuration > 0)
                                         .ToList())
                            {
                                audioInstruction.AudioDuration = null;
                                audioInstruction.AudioFileName = string.Empty;
                            }

                        // Once we have a list of to-dos, create the timers to start the sequence.
                        var topDuration = 0;

                        if (deviceInstructions.Count != 0)
                        {
                            foreach (var di in deviceInstructions)
                            {
                                var audioTop = di.AudioDuration.GetValueOrDefault();
                                var pinTop = di.PinDuration.GetValueOrDefault();

                                var top = di.OnAt + Math.Max(pinTop, audioTop);
                                if (top > topDuration)
                                    topDuration = top;

                                Timer timerStart = null;

                                timerStart = new Timer(instructionState =>
                                    {
                                        if (instructionState is DeviceInstructions item) SendInstruction(item);

                                        if (timerStart != null)
                                        {
                                            timerStart.Dispose();
                                            _queuedTimers.TryRemove(timerStart, out _);
                                        }
                                    },
                                    di,
                                    TimeSpan.FromMilliseconds(di.OnAt),
                                    Timeout.InfiniteTimeSpan);

                                // Tracks the timer, just in case we need to cancel everything.
                                _queuedTimers.TryAdd(timerStart, true);
                            }

                            // Setup the timer to say the set is not executing.
                            Timer stoppedTimer = null;
                            stoppedTimer = new Timer(x =>
                                {
                                    _logger.LogInformation("Set Execution Complete.");
                                    _setExecuting = false;
                                    stoppedTimer.Dispose();
                                },
                                null,
                                TimeSpan.FromMilliseconds(topDuration + delayBetweenSets),
                                Timeout.InfiniteTimeSpan);

                            _queuedTimers.TryAdd(stoppedTimer, true);
                        }
                        else
                        {
                            _logger.LogInformation("No patterns in set. Sleeping for one second");
                            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                            _setExecuting = false;
                        }

                        _logger.LogInformation($"Set Build Time: {setBuildSw.Elapsed}");
                    }
                }

                // The loop will re-check state every 250 milliseconds
                if (_setExecuting)
                    await Task.Delay(250, stoppingToken).ConfigureAwait(false);
                else
                    await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in RunServer loop.");
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                _setExecuting = false;
            }
    }

    private bool IsCurrentTimeBetweenSettingTimes(string startTime, string endTime)
    {
        if (!TimeSpan.TryParse(startTime, out var start))
        {
            _logger.LogWarning($"Could not parse start time: {startTime}");
            return false;
        }

        if (!TimeSpan.TryParse(endTime, out var end))
        {
            _logger.LogWarning($"Could not parse end time: {endTime}");
            return false;
        }

        var now = DateTime.Now.TimeOfDay;

        if (start <= end)
        {
            // start and stop times are in the same day
            if (now >= start && now <= end)
                // current time is between start and stop
                return true;
        }
        else
        {
            // start and stop times are in different days
            if (now >= start || now <= end)
                // current time is between start and stop
                return true;
        }

        return false;
    }

    private void InitiateReset()
    {
        var timers = _queuedTimers.ToArray();
        foreach (var kv in timers)
            if (_queuedTimers.TryRemove(kv.Key, out _))
                kv.Key.Dispose();

        // Send messages to each client to Reset
        var r = new ProtocolMessage(MessageTypeIdEnum.Reset);
        foreach (var remoteClient in _clients) remoteClient.Value.SendMessageAsync(r);
    }

    private List<DeviceInstructions>? EffectRandom(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
        var duration = GetDuration(metadataKeyValue);

        if (duration == null) return null;
        if (devicesAndKey.Count == 0) return null;

        devicesAndKey.Shuffle();

        var startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var endingPosition = setDurationMs;

        if (setSequence.DeviceEffects.Duration > 0)
            endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;

        while (startingPoint < endingPosition)
        {
            foreach (var dk in devicesAndKey)
            {
                if (startingPoint + duration.Value > endingPosition.Value)
                    goto end;

                var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                {
                    OnAt = startingPoint,
                    CommandPin = dk.Value,
                    PinDuration = duration
                };

                list.Add(di);
                startingPoint += duration.Value * 2;
            }

            devicesAndKey.Shuffle();
        }

        end:
        return list;
    }

    private List<DeviceInstructions>? EffectRandomDelay(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
        var duration = GetDuration(metadataKeyValue);

        if (duration == null) return null;

        const string keyDelayBetween = "DELAYBETWEEN";
        if (!metadataKeyValue.TryGetValue(keyDelayBetween, out var delayBetweenStr) ||
            !uint.TryParse(delayBetweenStr, out var delayBetween))
        {
            _logger.LogWarning("EffectRandomDelay missing or invalid DELAYBETWEEN");
            return null;
        }

        const string keyExecuteFor = "EXECUTEFOR";
        if (!metadataKeyValue.TryGetValue(keyExecuteFor, out var executeForStr) ||
            !uint.TryParse(executeForStr, out var executeFor))
        {
            _logger.LogWarning("EffectRandomDelay missing or invalid EXECUTEFOR");
            return null;
        }

        if (devicesAndKey.Count == 0) return null;

        devicesAndKey.Shuffle();

        long startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var endingPosition = setDurationMs;

        if (setSequence.DeviceEffects.Duration > 0)
            endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;

        var nextDelayAt = startingPoint + executeFor;

        while (startingPoint < endingPosition)
        {
            foreach (var dk in devicesAndKey)
            {
                if (startingPoint + duration.Value > endingPosition.Value)
                    goto end;

                var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                {
                    OnAt = (int)startingPoint,
                    CommandPin = dk.Value,
                    PinDuration = duration
                };

                list.Add(di);
                startingPoint += duration.Value * 2;

                if (startingPoint >= nextDelayAt)
                {
                    startingPoint += delayBetween;
                    nextDelayAt = startingPoint + executeFor;
                }
            }

            devicesAndKey.Shuffle();
        }

        end:
        return list;
    }

    private List<DeviceInstructions>? EffectStrobe(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
        var duration = GetDuration(metadataKeyValue);

        if (duration == null) return null;
        if (devicesAndKey.Count == 0) return null;

        var startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var endingPosition = setDurationMs;

        if (setSequence.DeviceEffects.Duration > 0)
            endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;

        while (startingPoint < endingPosition)
        {
            list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
            {
                OnAt = startingPoint,
                CommandPin = dk.Value,
                PinDuration = duration
            }));

            startingPoint += duration.Value * 2;
        }

        return list;
    }

    private List<DeviceInstructions>? EffectStrobeDelay(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
        var duration = GetDuration(metadataKeyValue);

        if (duration == null) return null;

        const string keyDelayBetween = "DELAYBETWEEN";
        if (!metadataKeyValue.TryGetValue(keyDelayBetween, out var delayBetweenStr) ||
            !uint.TryParse(delayBetweenStr, out var delayBetween))
        {
            _logger.LogWarning("EffectStrobeDelay missing or invalid DELAYBETWEEN");
            return null;
        }

        const string keyExecuteFor = "EXECUTEFOR";
        if (!metadataKeyValue.TryGetValue(keyExecuteFor, out var executeForStr) ||
            !uint.TryParse(executeForStr, out var executeFor))
        {
            _logger.LogWarning("EffectStrobeDelay missing or invalid EXECUTEFOR");
            return null;
        }

        if (devicesAndKey.Count == 0) return null;

        devicesAndKey.Shuffle();

        long startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var endingPosition = setDurationMs;

        if (setSequence.DeviceEffects.Duration > 0)
            endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;

        var nextDelayAt = startingPoint + executeFor;

        while (startingPoint < endingPosition)
        {
            list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
            {
                OnAt = (int)startingPoint,
                CommandPin = dk.Value,
                PinDuration = duration
            }));

            startingPoint += duration.Value * 2;
            if (startingPoint >= nextDelayAt)
            {
                startingPoint += delayBetween;
                nextDelayAt = startingPoint + executeFor;
            }
        }

        return list;
    }

    private List<DeviceInstructions>? EffectStayOn(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);

        if (devicesAndKey.Count == 0) return null;

        var startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var timeLeft = setDurationMs - setSequence.OnAt;

        if (setSequence.DeviceEffects.Duration > 0) timeLeft = setSequence.DeviceEffects.Duration;

        if (timeLeft <= 0) return list;

        list.AddRange(devicesAndKey.Select(dk => new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
        {
            OnAt = startingPoint,
            CommandPin = dk.Value,
            PinDuration = timeLeft
        }));

        return list;
    }

    private List<DeviceInstructions>? EffectSequential(SetSequences setSequence, int? setDurationMs,
        Dictionary<int, List<int>> disabledPins)
    {
        // Implementation as per original RunServer method
        var metadataKeyValue = GetMetaDataKeyValues(setSequence.DeviceEffects.InstructionMetaData);

        const string reverseKey = "REVERSE";

        var devicesAndKey = GetDevicesAndPins(metadataKeyValue, disabledPins);
        var duration = GetDuration(metadataKeyValue);

        if (duration == null) return null;

        var reverse = metadataKeyValue.ContainsKey(reverseKey) && metadataKeyValue[reverseKey] == "1";

        if (devicesAndKey.Count == 0) return null;

        var startingPoint = setSequence.OnAt;
        var list = new List<DeviceInstructions>();
        var endingPosition = setDurationMs;

        if (setSequence.DeviceEffects.Duration > 0)
            endingPosition = setSequence.OnAt + setSequence.DeviceEffects.Duration;

        while (startingPoint < endingPosition)
        {
            foreach (var dk in devicesAndKey)
            {
                if (startingPoint + duration.Value > endingPosition.Value)
                    goto end;

                var di = new DeviceInstructions(dk.Key, MessageTypeIdEnum.EventControl)
                {
                    OnAt = startingPoint,
                    CommandPin = dk.Value,
                    PinDuration = duration
                };

                list.Add(di);
                startingPoint += duration.Value * 2;
            }

            if (reverse)
                devicesAndKey.Reverse();
        }

        end:
        return list;
    }

    private int? GetDuration(Dictionary<string, string> metaDataKeyValues)
    {
        const string keyDur = "DUR";

        if (!metaDataKeyValues.TryGetValue(keyDur, out var dur))
        {
            _logger.LogWarning("DUR= not found in metadata");
            return null;
        }

        if (string.IsNullOrWhiteSpace(dur))
        {
            _logger.LogWarning("Pin Duration Expected. Must be formatted like DUR=50");
            return null;
        }

        if (int.TryParse(dur, out var duration)) return duration;

        _logger.LogWarning("Invalid duration: " + dur);
        return null;
    }

    private Dictionary<string, string> GetMetaDataKeyValues(string metaData)
    {
        var dataChunks = metaData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        return dataChunks.Select(dataChunk => dataChunk
                .Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(kvarry => kvarry.Length == 2)
            .ToDictionary(kvarry => kvarry[0], kvarry => kvarry[1]);
    }

    private List<KeyValuePair<int, int>> GetDevicesAndPins(Dictionary<string, string> metaDataKeyValue,
        Dictionary<int, List<int>> disabledPins)
    {
        var results = new List<KeyValuePair<int, int>>();

        const string keyDevpins = "DEVPINS";

        if (!metaDataKeyValue.TryGetValue(keyDevpins, out var devPins))
        {
            _logger.LogWarning("Invalid Device:Pin Must be formatted like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
            return results;
        }

        if (string.IsNullOrWhiteSpace(devPins))
        {
            _logger.LogWarning("Invalid Device:Pin Must be formatted like: DEVPINS=1:1,1:2,1:3,2:1,2:2,2:3");
            return results;
        }

        var subDevPinAry = devPins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var kv in subDevPinAry)
        {
            var devIdPin = kv.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (devIdPin.Length != 2) continue;
            if (!int.TryParse(devIdPin[0], out var deviceId)) continue;
            if (!int.TryParse(devIdPin[1], out var gpioPinId)) continue;

            if (!disabledPins.ContainsKey(deviceId) ||
                (disabledPins.ContainsKey(deviceId) && !disabledPins[deviceId].Contains(gpioPinId)))
                results.Add(new KeyValuePair<int, int>(deviceId, gpioPinId));
        }

        return results;
    }

    private void SendInstruction(DeviceInstructions de)
    {
        var client = _clients.Where(x => x.Value.DeviceId == de.DeviceId).Select(x => x.Value).FirstOrDefault();

        if (client == null) return;

        var dic = new Dictionary<string, string>();

        if (de.CommandPin != -1)
        {
            dic.Add(ProtocolMessage.PINON, "1");
            dic.Add(ProtocolMessage.DURATION, de.PinDuration.ToString());
            dic.Add(ProtocolMessage.PINID, de.CommandPin.ToString());
        }

        if (!string.IsNullOrWhiteSpace(de.AudioFileName)) dic.Add(ProtocolMessage.AUDIOFILE, de.AudioFileName);

        var message = new ProtocolMessage(de.DeviceCommand, dic);

        client.SendMessageAsync(message);
    }

    private void OnClientConnected(RemoteClient client)
    {
        // Implement Pub/Sub publish logic for client connection if needed
    }

    private void OnClientDisconnected(RemoteClient client)
    {
        // Implement Pub/Sub publish logic for client disconnection if needed
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping TCP Server...");
        _running = false;
        _listener.Stop();

        foreach (var client in _clients.Keys) client.Close();

        foreach (var timer in _queuedTimers.Keys) timer.Dispose();

        _queuedTimers.Clear();

        await base.StopAsync(cancellationToken);
    }
}

public class DeviceInstructions
{
    public DeviceInstructions(int deviceId, MessageTypeIdEnum deviceCommand)
    {
        DeviceId = deviceId;
        DeviceCommand = deviceCommand;
    }

    public MessageTypeIdEnum DeviceCommand { get; }
    public int DeviceId { get; }
    public int OnAt { get; set; }
    public int CommandPin { get; set; }
    public int? PinDuration { get; set; }
    public string AudioFileName { get; set; }
    public int? AudioDuration { get; set; }

    public override string ToString()
    {
        return $"Dev: {DeviceId}; OnAt: {OnAt}, CommandPin: {CommandPin}";
    }
}

public static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        var rng = new Random();
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}