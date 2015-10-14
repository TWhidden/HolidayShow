using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Windows.Devices.Gpio;
using HolidayShowEndpointUniversalApp.BaseClasses;
using HolidayShowEndpointUniversalApp.Containers;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowEndpointUniversalApp.Storage;
using HolidayShowLibUniversal.Services;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEndpointUniversalApp
{
    public class MainPageViewModel : ViewModelBase 
    {
        private readonly IResolverService _resolverService;
        private DelegateCommand _commandSave;
        private readonly AppSettings _setting;
        private MainPage _view;
        private ClientLiveControl _protocolClient;
        private List<GpioPin> _availablePins;

        public MainPageViewModel(MainPage mainPage, IResolverService resolverService)
        {
            _resolverService = resolverService;
            _setting = AppSettings.Load();

            View = mainPage;
        }

        public MainPage View
        {
            get { return _view; }
            set
            {
                if (SetProperty(ref _view, value))
                {
                    if (value != null)
                    {
                        value.Loaded += Value_Loaded;
                    }
                }
            }
        }

        private void Value_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Identify the pings available.
            // Get the default GPIO controller on the system
            var gpio = GpioController.GetDefault();
            if (gpio == null)
                return; // GPIO not available on this system

            // Reference: https://ms-iot.github.io/content/en-US/win10/samples/PinMappingsRPi2.htm
            //Pin Added #4 - Gpio#: 4
            //Pin Added #5 - Gpio#: 5
            //Pin Added #6 - Gpio#: 6
            //Pin Added #12 - Gpio#: 12
            //Pin Added #13 - Gpio#: 13
            //Pin Added #16 - Gpio#: 16
            //Pin Added #18 - Gpio#: 18
            //Pin Added #22 - Gpio#: 22
            //Pin Added #23 - Gpio#: 23
            //Pin Added #24 - Gpio#: 24
            //Pin Added #25 - Gpio#: 25
            //Pin Added #26 - Gpio#: 26
            //Pin Added #27 - Gpio#: 27
            //Pin Added #35 - Gpio#: 35  // Not a GPIO pin we can use (Red LED)
            //Pin Added #47 - Gpio#: 47  // Not a GPIO pin we can use (Green LED)

            var blockedIds = new[] {35, 47};

            _availablePins = new List<GpioPin>();

            var pinCount = gpio.PinCount;

            for (var i = 0; i < pinCount; i++)
            {
                GpioPin pin;
                GpioOpenStatus status;
                if (gpio.TryOpenPin(i, GpioSharingMode.Exclusive, out pin, out status))
                {
                    if (blockedIds.Contains(pin.PinNumber)) continue; // Dont process blocked pins.

                    var supportsOutputMode = pin.IsDriveModeSupported(GpioPinDriveMode.Output);
                    if (supportsOutputMode)
                    {
                        _availablePins.Add(pin);                    // This is a pin we can use.
                        pin.SetDriveMode(GpioPinDriveMode.Output);  // We need this set to Output
                        pin.Write(GpioPinValue.Low);                // Init with a low value.
                        pin.DebounceTimeout = TimeSpan.Zero;        // Not all GPIO pins have this value set. 
                        Debug.WriteLine("Pin Added #{0} - Gpio#: {1}", i, pin.PinNumber);
                    }
                }
            }

            CreateClient();
        }

        private void CreateClient()
        {
            DestroyClient();

            if (DeviceId != 0)
            {
                var endPoint = new DnsEndPoint(ServerAddress, ServerPort);
                var serverDetails = _resolverService.Resolve<ServerDetails>(endPoint);
                _resolverService.Register<IServerDetails, IServerDetails>(serverDetails);

                _protocolClient = _resolverService.Resolve<ClientLiveControl>(DeviceId, _availablePins);
            }
                
        }

        private void DestroyClient()
        {
            _protocolClient?.Disconnect(false);
        }

        public int DeviceId
        {
            get { return _setting.DeviceId; }
            set
            {
                _setting.DeviceId = value;
                OnPropertyChanged(() => DeviceId);

                // temp for debug
                _setting.Save();
            }
        }

        public string ServerAddress
        {
            get { return _setting.ServerAddress; }
            set
            {
                _setting.ServerAddress = value;
                OnPropertyChanged(()=>ServerAddress);
            }
        }

        public ushort ServerPort
        {
            get { return _setting.ServerPort; }
            set
            {
                _setting.ServerPort = value;
                OnPropertyChanged(()=>ServerPort);
            }
        }

        public DelegateCommand CommandSave => _commandSave ?? (_commandSave = new DelegateCommand(OnCommandSave));

        private void OnCommandSave()
        {
            _setting.Save();
            CreateClient();
        }
    }
}
