using System;
using System.Threading.Tasks;
#if CORE
using Unosquare.RaspberryIO.Gpio;
#else
using Windows.Devices.Gpio;
#endif

namespace HolidayShowEndpointUniversalApp.Containers
{
    public class OutletControl
    {
        private GpioPin _gpioPin;
        private byte? _piRelayPin;

        public OutletControl(GpioPin gpioPin)
        {
            _gpioPin = gpioPin;
        }

        public OutletControl(byte piRelayPin)
        {
            _piRelayPin = piRelayPin;
        }

        public void TurnOn()
        {
            if (_piRelayPin.HasValue)
            {
#if CORE
                PiRelayPlate.NetCore.RelayPlate.SetPinState(_piRelayPin.Value, 1);
#else
                PiPlateRelay.PiRelay.RelayOnAsync(_piRelayPin.Value);
#endif
            }
            else
            {
                _gpioPin?.Write(GpioPinValue.High);
            }
        }

        public void TurnOff()
        {
            if (_piRelayPin.HasValue)
            {
#if CORE
                PiRelayPlate.NetCore.RelayPlate.SetPinState(_piRelayPin.Value, 0);
#else
                PiPlateRelay.PiRelay.RelayOffAsync(_piRelayPin.Value);
#endif
            }
            else
            {
                _gpioPin?.Write(GpioPinValue.Low);
            }
        }

        public int PinNumber => _piRelayPin ?? _gpioPin.PinNumber;
    }
}
