using HolidayShow.Data.Core;
using HolidayShowServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

// Fallback route for client-side routing
app.MapFallbackToFile("index.html");

app.Run();