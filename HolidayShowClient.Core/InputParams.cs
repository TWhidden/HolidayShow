using CommandLine;

namespace HolidayShowClient.Core
{
    public class InputParams
    {
        [Option('p', "port", HelpText = "Server Port", Default = 5555)]
        public int ServerPort { get; set; }

        [Option('s', "server", HelpText = "Server Address", Required = true)]
        public string Server { get; set; }

        [Option('a', "storagepath", HelpText = "Path to store storage such as audio files", Required = true)]
        public string StoragePath { get; set; }

        [Option('d', "deviceid", HelpText = "The unique ID assigned to this device", Required = true)]
        public int DeviceId { get; set; }
    }
}
