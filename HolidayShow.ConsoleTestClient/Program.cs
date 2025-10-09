using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using HolidayShowLib;
using HolidayShow.ConsoleTestClient.Containers;
using HolidayShow.ConsoleTestClient.Services;

namespace HolidayShow.ConsoleTestClient;

internal class Program
{
    private static TestClient? _testClient;
    public static string? ServerAddress { get; private set; }
    public static int ServerPort { get; private set; }
    public static int DeviceId { get; private set; }

    private static void Main(string[] args)
    {
        var result = Parser.Default.ParseArguments<InputParams>(args);

        var exitCode = result.MapResult
        (
            options =>
            {
                ServerPort = options.ServerPort;
                ServerAddress = options.Server;
                DeviceId = options.DeviceId;
                return 0;
            },
            errors =>
            {
                Console.WriteLine("Error parsing arguments:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  {error}");
                }
                Console.WriteLine("\nUsage: HolidayShow.ConsoleTestClient [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -s, --server     Server Address (default: localhost)");
                Console.WriteLine("  -p, --port       Server Port (default: 5555)");
                Console.WriteLine("  -d, --deviceid   Device ID (default: 1)");
                return 1;
            }
        );

        if (exitCode == 1) return;

        Console.WriteLine("🎄 HolidayShow Console Test Client 🎄");
        Console.WriteLine($"Device ID: {DeviceId}");
        Console.WriteLine($"Server: {ServerAddress}:{ServerPort}");
        Console.WriteLine();
        Console.WriteLine("Press any key to connect to the server...");
        Console.ReadKey(true);
        Console.WriteLine();

        ConnectLoop();
    }

    public static bool _clientConnected = false;

    private static void ConnectLoop()
    {
        while (true)
        {
            try
            {
                CreateClient();
                Console.WriteLine("Press Ctrl+C to exit...");
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    _testClient?.Disconnect(false);
                    Environment.Exit(0);
                };

                // Keep running until disconnected
                while (_testClient != null && _clientConnected)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
            }

            Console.WriteLine("⏳ Waiting 2 seconds before retrying...");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine("🔄 Retrying connection...");
        }
    }

    private static void CreateClient()
    {
        Console.WriteLine("🔌 Connecting to server...");

        var endPoint = new DnsEndPoint(ServerAddress!, ServerPort);
        var serverDetails = new ServerDetails(endPoint);

        _testClient = new TestClient(serverDetails, DeviceId);
    }
}