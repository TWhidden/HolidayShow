using CommandLine;

namespace HolidayShow.ConsoleTestClient;

public class InputParams
{
    [Option('p', "port", HelpText = "Server Port", Default = 5556)]
    public int ServerPort { get; set; }

    [Option('s', "server", HelpText = "Server Address", Default = "localhost")]
    public string? Server { get; set; }

    [Option('d', "deviceid", HelpText = "The unique ID assigned to this device", Default = 1)]
    public int DeviceId { get; set; }
}