using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HolidayShowWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // .UseUrls($"http://{Options.WebHost}:{Options.WebPort}")
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            // get the environment var
            var portStr = System.Environment.GetEnvironmentVariable("PORT");
            if (!int.TryParse(portStr, out var port))
            {
                port = 8000;
            }

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls($"http://*:{port}");
        }


    }
}
