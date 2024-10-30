using GPSLocator.Data;
using GPSLocator.Filters;
using GPSLocator.Hubs;
using GPSLocator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GPSService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<APIKeyFilter>();

builder.Services.AddScoped<GPSService>();

var serviceProvider = builder.Services.AddDbContext<GPSLocatorContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))).BuildServiceProvider();

using (var context = serviceProvider.GetRequiredService<GPSLocatorContext>())
{
	context.Database.EnsureCreated();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<GPSHub>("/gpsHub");

app.Run();
