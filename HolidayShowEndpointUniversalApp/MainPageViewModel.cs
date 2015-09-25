using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Gpio;
using Windows.Networking;
using HolidayShowEndpoint;
using HolidayShowEndpointUniversalApp.BaseClasses;
using HolidayShowEndpointUniversalApp.Storage;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEndpointUniversalApp
{
    public class MainPageViewModel : ViewModelBase 
    {
        public DelegateCommand _commandSave;
        private AppSettings _setting;
        private MainPage _view;
        private int _deviceId;
        private Client _client;
        private List<GpioPin> _availablePins;

        public MainPageViewModel()
        {
            _setting = AppSettings.Load();
            
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

            _availablePins = new List<GpioPin>();


            var pinCount = gpio.PinCount;

            for (var i = 0; i < pinCount; i++)
            {
                GpioPin pin;
                GpioOpenStatus status;
                if (gpio.TryOpenPin(i, GpioSharingMode.Exclusive, out pin, out status))
                {
                    var supportsOutputMode = pin.IsDriveModeSupported(GpioPinDriveMode.Output);
                    if (supportsOutputMode)
                    {
                        _availablePins.Add(pin);                    // This is a pin we can use.
                        pin.SetDriveMode(GpioPinDriveMode.Output);  // We need this set to Output
                        pin.Write(GpioPinValue.Low);                // Init with a low value.
                        pin.DebounceTimeout = TimeSpan.Zero;        // Not all GPIO pins have this value set. 
                    }
                    
                }
            }

            CreateClient();
        }

        private void CreateClient()
        {
            DestroyClient();

            if(DeviceId != 0)
                _client = new Client(new DnsEndPoint(ServerAddress, (int)ServerPort), DeviceId, _availablePins);
        }

        private void DestroyClient()
        {
            _client?.Disconnect(false);
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

        public DelegateCommand CommandSave
        {
            get { return _commandSave ?? (_commandSave = new DelegateCommand(OnCommandSave)); }
        }

        private void OnCommandSave()
        {
            _setting.Save();
            CreateClient();
        }
    }
}
