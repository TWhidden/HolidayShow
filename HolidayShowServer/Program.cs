using HolidayShow.Data.Core;
using HolidayShowServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DeviceStatusService>();
builder.Services.AddHostedService<TcpBackgroundService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EfHolidayContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (Vite build output should be in wwwroot)
app.UseStaticFiles();

// Configure SPA routing
app.UseSpa(spa =>
{
    spa.Options.SourcePath = Path.Join(app.Environment.ContentRootPath, "../HolidayShowWeb");

    if (app.Environment.IsDevelopment())
    {
        // Set longer timeout for Vite startup
        spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
    }
});

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();
app.MapHub<DeviceStatusHub>("/websocket/deviceStatusHub");

// Fallback route for client-side routing
app.MapFallbackToFile("index.html");

app.Run();