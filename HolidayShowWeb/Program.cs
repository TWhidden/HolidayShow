using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace HolidayShowWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // .UseUrls($"http://{Options.WebHost}:{Options.WebPort}")
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls($"http://*:5001");

    }
}
