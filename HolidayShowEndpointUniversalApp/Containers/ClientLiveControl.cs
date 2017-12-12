﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Devices.Gpio;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowLib;
using HolidayShowLibUniversal.Controllers;
using HolidayShowLibUniversal.Services;

namespace HolidayShowEndpointUniversalApp.Containers
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
            // FOr pin control.
            if (message.MessageParts.ContainsKey(ProtocolMessage.PINID) &&
                message.MessageParts.ContainsKey(ProtocolMessage.DURATION) &&
                message.MessageParts.ContainsKey(ProtocolMessage.PINON))
            {
                int pinIndex;
                var parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINID], out pinIndex);
                if (!parsed) return;

                pinIndex--; // not zero based when its received.

                int durration;
                parsed = int.TryParse(message.MessageParts[ProtocolMessage.DURATION], out durration);
                if (!parsed) return;

                int on;
                parsed = int.TryParse(message.MessageParts[ProtocolMessage.PINON], out on);
                if (!parsed) return;

                if (pinIndex >= _availablePins.Count) return;

                var gpioPin = _availablePins[pinIndex];

                if (on == 1)
                {
                    await gpioPin.TurnOn();
                }
                else
                {
                    await gpioPin.TurnOff();
                }

                if (durration > 0)
                {
                    Timer timer;
                    if (_rootedTimer.TryRemove(gpioPin, out timer))
                    {
                        timer.Dispose();
                    }
                    
                    timer = new Timer(async x =>
                        {
                            await gpioPin.TurnOff();

                        lock (_rootedTimer)
                        {
                            if (_rootedTimer.ContainsKey(gpioPin))
                            {
                                _rootedTimer[gpioPin].Dispose();
                                Timer t;
                                _rootedTimer.TryRemove(gpioPin, out t);
                            }
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

        public async void AllOff()
        {
            // stops all the running audio.
            RunningAudioFiles.ToList().ForEach(x => x.Stop());

            foreach (var pin in _availablePins)
            {
                await pin.TurnOff();
            }

            var audioControllers = _resolverService.Resolve<IAudioManagerController>();
            audioControllers.StopAllAudio();
        }

        private void SetPin(GpioPin pin, GpioPinValue value)
        {
            Debug.WriteLine("Pin {0} value {1}", pin.PinNumber, value);
            pin.Write(value);
        }
    }
}
