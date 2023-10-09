using CommandLine;

namespace HolidayShowServer.Core
{
    public class InputParams
    {
        [Option('p', "port", HelpText = "Server Port", Default = 5555)]
        public int ServerPort { get; set; }

        [Option('d', "dbserver", HelpText = "Database Server Address (127.0.0.1,1433)", Default = ".")]
        public string DbServer { get; set; }

        [Option('n', "dbname", HelpText = "Database Name (ie: holidayshow)", Default = "HolidayShow")]
        public string DbName { get; set; }

        [Option('u', "user", HelpText = "Database username. Use TRUSTED for windows auth", Default = "holidayshow")]
        public string Username { get; set; }

        [Option('s', "password", HelpText = "Database password. If trusted, leave blank", Default = "")]
        public string Password { get; set; }
    }
}
