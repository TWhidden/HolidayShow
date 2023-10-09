using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HolidayShowClient.Core.Services;
using HolidayShowLib;
using HolidayShowLibShared.Core.Services;
using HolidayShowLibUniversal.Controllers;

namespace HolidayShowClient.Core.Containers
{
    public class ClientLiveControl : ProtocolClient
    {
        private readonly int _deviceId;
        private readonly IResolverService _resolverService;
        private readonly List<OutletControl> _availablePins;

        protected static readonly List<IAudioRequestController> RunningAudioFiles = new List<IAudioRequestController>();
        private readonly ConcurrentDictionary<OutletControl, Timer> _rootedTimer = new ConcurrentDictionary<OutletControl, Timer>();

        public ClientLiveControl(IServerDetails serverDetails, int deviceId, List<OutletControl> availablePins, IResolverService resolverService) : base(serverDetails)
        {
            _deviceId = deviceId;
            _resolverService = resolverService;
            _availablePins = availablePins;
        }

        public override void FileRequestReceived(ProtocolMessage message)
        {
            // Not Used
        }

        protected override void NewConnectionEstablished()
        {
            foreach (var timer in _rootedTimer.Values.ToList())
            {
                timer.Dispose();
            }
            _rootedTimer.Clear();

            // Resets all the lights to off to prepare for the new connection/sets
            AllOff();

            var dic = new Dictionary<string, string>
                {
                    {ProtocolMessage.DEVID, _deviceId.ToString()},
                    {ProtocolMessage.PINSAVAIL, _availablePins.Count.ToString()},
                    {ProtocolMessage.PINNAMES, string.Join(",", _availablePins.Select(x => "GPIO #" + x.PinNumber).ToArray()) }
                };

            var message = new ProtocolMessage(MessageTypeIdEnum.DeviceId, dic);
            BeginSend(message);
        }

        protected override void ErrorDetected(Exception ex)
        {
            AllOff();
        }

        protected override void ResetReceived()
        {
            AllOff();
        }

        protected override async void EventControlReceived(ProtocolMessage message)
        {

            Console.WriteLine($"Message: {string.Join(", ", message.MessageParts.Select(y => $"{y.Key}={y.Value}"))}");
            // FOr pin control.
            if (message.MessageParts.ContainsKey(ProtocolMessage.PINID) &&
                message.MessageParts.ContainsKey(ProtocolMessage.DURATION) &&
                message.MessageParts.ContainsKey(ProtocolMessage.PINON))
            {
                var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINID], out var pinIndex);
                if (!parsed)
                {
                    Console.WriteLine($"PINID '{message.MessageParts[ProtocolMessage.PINID]}' could not be parsed!");
                    return;
                }

                if (pinIndex > _availablePins.Count)
                {
                    Console.WriteLine($"pinIndex {pinIndex} is greater than pin count {_availablePins.Count}");
                    return;
                }

                parsed = int.TryParse(message.MessageParts[ProtocolMessage.DURATION], out var durration);
                if (!parsed)
                {
                    Console.WriteLine($"DURATION '{message.MessageParts[ProtocolMessage.DURATION]}' could not be parsed!");
                    return;
                }

                parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINON], out var @on);
                if (!parsed)
                {
                    Console.WriteLine($"PINON '{message.MessageParts[ProtocolMessage.PINON]}' could not be parsed!");
                    return;
                }

                var gpioPin = _availablePins[pinIndex - 1]; // not zero based when its received.

                if (on == 1)
                {
                    gpioPin.TurnOn();
                }
                else
                {
                    gpioPin.TurnOff();
                }

                if (durration > 0)
                {
                    if (_rootedTimer.TryRemove(gpioPin, out var timer))
                    {
                        timer.Dispose();
                    }
                    
                    timer = new Timer(x =>
                        {
                            gpioPin.TurnOff();

                        lock (_rootedTimer)
                        {
                            if (!_rootedTimer.ContainsKey(gpioPin)) return;
                            _rootedTimer[gpioPin].Dispose();
                            _rootedTimer.TryRemove(gpioPin, out _);
                        }

                    },
                                      null,
                                      TimeSpan.FromMilliseconds(durration),
                                      TimeSpan.FromMilliseconds(-1));
                    lock (_rootedTimer)
                    {
                        if (_rootedTimer.ContainsKey(gpioPin))
                        {
                            _rootedTimer[gpioPin].Dispose();
                            _rootedTimer[gpioPin] = timer;
                        }
                        else
                        {
                            _rootedTimer.TryAdd(gpioPin, timer);
                        }
                    }
                }
            }

            if (message.MessageParts.ContainsKey(ProtocolMessage.AUDIOFILE))
            {
                var audioFile = message.MessageParts[ProtocolMessage.AUDIOFILE];
                if (!string.IsNullOrWhiteSpace(audioFile))
                {
                    var managerController = _resolverService.Resolve<IAudioManagerController>();
                    var controller = await managerController.RequestAndPlay(audioFile);
                    if(controller != null)
                    {
                        controller.OnStop += ((s, e) =>
                        {
                            RunningAudioFiles.Remove(controller);
                        });
                        RunningAudioFiles.Add(controller);
                    }
                }
            }
        }

        public void AllOff()
        {
            // stops all the running audio.
            RunningAudioFiles.ToList().ForEach(x => x.Stop());

            foreach (var pin in _availablePins)
            {
                pin.TurnOff();
            }

            var audioControllers = _resolverService.Resolve<IAudioManagerController>();
            audioControllers.StopAllAudio();
        }
    }
}
