using System;
using System.IO;
using HolidayShow.Data;
using HolidayShowWeb.Resovlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace HolidayShowWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var server = System.Environment.GetEnvironmentVariable("DBSERVER");
            var database = System.Environment.GetEnvironmentVariable("DBNAME");
            var username = System.Environment.GetEnvironmentVariable("DBUSER");
            var password = System.Environment.GetEnvironmentVariable("DBPASS");

            var connectionString = Configuration.GetConnectionString("EfHolidayContext");
            if (!string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database={database};User Id={username};Password={password};";
            }
            else
            {
                throw new Exception("Environment Variables Not Set! Set for DB Connection Information!");
            }

            services.AddDbContext<EfHolidayContext>(options =>
                options.UseSqlServer(connectionString));

            // Update the database
            using (var dc = new EfHolidayContext(connectionString))
            {
                dc.UpdateDatabase();
            }

            // needed to tell the system where the Single Page App files are located.
            services.AddSpaStaticFiles((x) => { x.RootPath = "wwwroot/react"; });


            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options => {
                //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                options.SerializerSettings.ContractResolver = new EnityFrameworkCustomResolver();
            });

            // Register the web controllers
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });

            // set default files like index.html
            app.UseDefaultFiles();

            // Enable SPA files for single pages (react)
            app.UseSpaStaticFiles();

            // Specify the location of the react build files
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = Path.Join(env.ContentRootPath, "wwwroot/react");
            });

            



        }
    }
}
