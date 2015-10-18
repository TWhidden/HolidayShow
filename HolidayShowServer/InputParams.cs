using CommandLine;

namespace HolidayShowServer
{
    public class InputParams
    {
        [Option('p', "port", HelpText = "Server Port", Default = 5555)]
        public int ServerPort { get; set; }
    }
}
