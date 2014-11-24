using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace HolidayShowEndpoint
{
    public class InputParams
    {
        [Option('s', "server", HelpText = "Server Address", DefaultValue = "10.64.128.150")]
        public string ServerAddress { get; set; }

        [Option('p', "port", HelpText = "Server Port", DefaultValue = 5555)]
        public int ServerPort { get; set; }

        [Option('d', "deviceid", Required = true, HelpText = "This device ID")]
        public int DeviceId { get; set; }

        [Option('s', "skipinit", HelpText = "Skip the light init for faster startup.")]
        public bool SkipLightInit { get; set; }
    }
}
