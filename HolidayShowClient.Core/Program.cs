using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using HolidayShowEndpointUniversalApp.Containers;
using HolidayShowEndpointUniversalApp.Controllers;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowLibUniversal.Controllers;
using HolidayShowLibUniversal.Services;
using Unosquare.RaspberryIO.Gpio;

namespace HolidayShowClient.Core
{
    class Program
    {
        public static string ServerAddress { get; private set; }

        public static int ServerPort { get; private set; }

        public static string StoragePath { get; private set; }

        public static int DeviceId { get; private set; }

        public static IResolverService ResloverService { get; private set; }

        private static ClientLiveControl _protocolClient;
        private static List<OutletControl> _availablePins;

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<InputParams>(args);

            var exitCode = result.MapResult
            (
                options =>
                {
                    ServerPort = options.ServerPort;
                    ServerAddress = options.Server;
                    StoragePath = options.StoragePath;
                    DeviceId = options.DeviceId;
                    return 0;
                },
                errors =>
                {
                    Console.WriteLine(errors);
                    return 1;
                }
            );

            if (exitCode == 1) return;



            // For now, this is where the ResolverService will be created with the registered singleton
            ResloverService = new ResolverService();
            ResloverService.Register<IResolverService, IResolverService>(ResloverService);

            // Type used to register a request for audio playback
            ResloverService.Register<IAudioRequestController, AudioRequestController>();

            var availableInstances = new List<IAudioInstanceController>();
            // Create the audio instances
            var audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media0);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media1);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media2);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media3);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media4);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media5);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media6);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media7);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media8);
            availableInstances.Add(audioInstance);

            audioInstance = ResloverService.Resolve<AudioInstanceController>();
            //audioInstance.SetMediaElement(Media9);
            availableInstances.Add(audioInstance);


            // Create the manager controller
            var audioManagerController = ResloverService.Resolve<AudioManagerController>(availableInstances);
            ResloverService.Register<IAudioManagerController, IAudioManagerController>(audioManagerController);

            var t = Load();
            t.Wait();

            Console.WriteLine("Press enter to end program");
            Console.ReadLine();
        }

        private async static Task Load()
        {
            _availablePins = new List<OutletControl>();

            // Identify if the device has a PiPlate attached.
#if !CORE
            await PiPlateRelay.PiRelay.InititlizeAsync();
#endif

#if CORE
            var relaysAvailable = PiRelayPlate.NetCore.RelayPlate.RelaysAvailable();
            if (relaysAvailable > 0)
            {
                // build up the ports
                var portsAvailable = relaysAvailable * 7;
                for (byte portIndex = 1; portIndex <= portsAvailable; portIndex++)
                {
                    _availablePins.Add(new OutletControl(portIndex));
                }
            }
#else
            if (PiPlateRelay.PiRelay.RelaysAvailable.Count > 0)
            {
                foreach (var kv in PiPlateRelay.PiRelay.RelaysAvailable)
                {
                    for (byte outlet = 1; outlet <= 7; outlet++)
                    {
                        _availablePins.Add(new OutletControl((byte)((kv.Key * 7) + outlet)));
                    }
                }
            }
#endif

            else
            {

                // Identify the pings available.
                // Get the default GPIO controller on the system
#if CORE
                var gpio = GpioController.Instance;
#else
                var gpio = GpioController.GetDefault();
#endif

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

                var blockedIds = new[] { 35, 47 };

#if CORE
                var pinCount = gpio.Count;
#else
                var pinCount = gpio.PinCount;
#endif

                for (var i = 0; i < pinCount; i++)
                {
                    var pin = gpio[i];
                    if (blockedIds.Contains(pin.BcmPinNumber)) continue;
                    if (!pin.Capabilities.Contains(PinCapability.GP)) continue;
                    pin.PinMode = GpioPinDriveMode.Output;
                    pin.Write(GpioPinValue.Low);
                    _availablePins.Add(new OutletControl(pin)); // This is a pin we can use.

                    //GpioOpenStatus status;
                    //if (!gpio.TryOpenPin(i, GpioSharingMode.Exclusive, out GpioPin pin, out status)) continue;
                    //if (blockedIds.Contains(pin.PinNumber)) continue; // Dont process blocked pins.

                    //var supportsOutputMode = pin.IsDriveModeSupported(GpioPinDriveMode.Output);
                    //if (!supportsOutputMode) continue;
                    //_availablePins.Add(new OutletControl(pin)); // This is a pin we can use.
                    //pin.SetDriveMode(GpioPinDriveMode.Output); // We need this set to Output
                    //pin.Write(GpioPinValue.Low); // Init with a low value.
                    //pin.DebounceTimeout = TimeSpan.Zero; // Not all GPIO pins have this value set. 
                    //Debug.WriteLine("Pin Added #{0} - Gpio#: {1}", i, pin.PinNumber);
                }
            }

            CreateClient();
        }

        private static void CreateClient()
        {
            DestroyClient();

            if (DeviceId != 0)
            {
                var endPoint = new DnsEndPoint(ServerAddress, ServerPort);
                var serverDetails = ResloverService.Resolve<ServerDetails>(endPoint);
                ResloverService.Register<IServerDetails, IServerDetails>(serverDetails);

                _protocolClient = ResloverService.Resolve<ClientLiveControl>(DeviceId, _availablePins);
            }

        }

        private static void DestroyClient()
        {
            _protocolClient?.Disconnect(false);
        }
    }
}
