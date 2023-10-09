using Unosquare.RaspberryIO.Abstractions;

namespace HolidayShowClient.Core.Containers
{
    public class OutletControl
    {
        private IGpioPin _gpioPin;
        private byte? _piRelayPin;

        public OutletControl(IGpioPin gpioPin)
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
                PiRelayPlate.NetCore.RelayPlate.SetPinState(_piRelayPin.Value, 1);

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

                PiRelayPlate.NetCore.RelayPlate.SetPinState(_piRelayPin.Value, 0);

            }
            else
            {
                _gpioPin?.Write(GpioPinValue.Low);
            }
        }

        public int PinNumber => _piRelayPin ?? _gpioPin.PhysicalPinNumber;
    }
}
