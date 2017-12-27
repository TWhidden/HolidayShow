using System.Threading.Tasks;
using Windows.Devices.Gpio;

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
                PiPlateRelay.PiRelay.RelayOnAsync(_piRelayPin.Value);
            }else
            {
                _gpioPin?.Write(GpioPinValue.High);
            }
        }

        public void TurnOff()
        {
            if (_piRelayPin.HasValue)
            {
                PiPlateRelay.PiRelay.RelayOffAsync(_piRelayPin.Value);
            }
            else
            {
                _gpioPin?.Write(GpioPinValue.Low);
            }
        }

        public int PinNumber => _piRelayPin ?? _gpioPin.PinNumber;
    }
}
