using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using HolidayShowClient.Core.Containers;
using HolidayShowClient.Core.Controllers;
using HolidayShowClient.Core.Services;
using HolidayShowLibShared.Core.Services;
using HolidayShowLibUniversal.Controllers;
using PiRelayPlate.NetCore;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace HolidayShowClient.Core;

internal class Program
{
    private static ClientLiveControl _protocolClient;
    private static List<OutletControl> _availablePins;
    public static string ServerAddress { get; private set; }

    public static int ServerPort { get; private set; }

    public static string StoragePath { get; private set; }

    public static int DeviceId { get; private set; }

    public static IResolverService ResloverService { get; private set; }

    private static void Main(string[] args)
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

        Thread.Sleep(Timeout.Infinite);

        if (Environment.UserInteractive)
        {
            Console.WriteLine("Press [ENTER] to stop the server");
            Console.ReadLine();
        }
        else
        {
            // Docker has no interactive mode. If there is no console, we need to sleep forever until the task is force quit.
            Console.WriteLine("End Task to stop the server");
            Thread.Sleep(Timeout.Infinite);
        }

        Console.WriteLine($"SHUTTING DOWN! {DateTime.Now}");
    }

    private static async Task Load()
    {
        Console.WriteLine("Loading...");
        _availablePins = new List<OutletControl>();

        // Identify if the device has a PiPlate attached.

        var relaysAvailable = 0;
        Console.WriteLine("Finding what Relays are Available...");
        try
        {
            relaysAvailable = RelayPlate.RelaysAvailable();
            Console.WriteLine($"Relays Found: {relaysAvailable}");
            if (relaysAvailable > 0)
            {
                // build up the ports
                var portsAvailable = relaysAvailable * 7;
                for (byte portIndex = 1; portIndex <= portsAvailable; portIndex++)
                    _availablePins.Add(new OutletControl(portIndex));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load pi relays. Error: {ex.Message}");
        }

        try
        {
            if (relaysAvailable == 0)
            {
                Console.WriteLine("Loading GPIO Instance");
                // Identify the pings available.
                // Get the default GPIO controller on the system

                var gpio = Pi.Gpio;


                ; // GPIO not available on this system

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

                var pinCount = gpio.Count;


                for (var i = 0; i < pinCount; i++)
                {
                    var pin = gpio[i];
                    if (blockedIds.Contains(pin.PhysicalPinNumber)) continue;
                    if (!IsGpioPin(pin)) return;
                    //if (!pin.Capabilities.Contains(PinCapability.GP)) continue;
                    pin.PinMode = GpioPinDriveMode.Output;
                    pin.Write(GpioPinValue.Low);
                    _availablePins.Add(new OutletControl(pin)); // This is a pin we can use.
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load GPIO. Error: {ex.Message}");
        }

        CreateClient();
    }

    private static bool IsGpioPin(IGpioPin pin)
    {
        try
        {
            pin.PinMode = GpioPinDriveMode.Input;
            pin.PinMode = GpioPinDriveMode.Output;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void CreateClient()
    {
        Console.WriteLine("Creating Client to connect");

        DestroyClient();

        if (DeviceId == 0) return;

        var endPoint = new DnsEndPoint(ServerAddress, ServerPort);
        Console.WriteLine($"Connecting to {endPoint.Host}:{endPoint.Port}");
        var serverDetails = ResloverService.Resolve<ServerDetails>(endPoint);
        ResloverService.Register<IServerDetails, IServerDetails>(serverDetails);

        Console.WriteLine("Starting TCP Client");
        _protocolClient = ResloverService.Resolve<ClientLiveControl>(DeviceId, _availablePins);
        Console.WriteLine("Client Started.");
    }

    private static void DestroyClient()
    {
        Console.WriteLine("Destroying any existing client...");
        _protocolClient?.Disconnect(false);
    }
}