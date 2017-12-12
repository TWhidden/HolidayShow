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

        public async Task TurnOn()
        {
            if (_piRelayPin.HasValue)
            {
                await PiPlateRelay.PiRelay.RelayOnAsync(_piRelayPin.Value);
            }else
            {
                _gpioPin?.Write(GpioPinValue.High);
            }
        }

        public async Task TurnOff()
        {
            if (_piRelayPin.HasValue)
            {
                await PiPlateRelay.PiRelay.RelayOffAsync(_piRelayPin.Value);
            }
            else
            {
                _gpioPin?.Write(GpioPinValue.Low);
            }
        }

        public int PinNumber => _piRelayPin ?? _gpioPin.PinNumber;
    }
}
