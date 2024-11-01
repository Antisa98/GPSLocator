using GPSLocator.Composition;
using GPSLocator.Data;
using GPSLocator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterAplicationDependencies(builder.Configuration);

// Build the app
var app = builder.Build();

// Initialize and migrate the database
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<GPSLocatorContext>();
	dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authorization and authentication (if needed)
app.UseAuthorization();

app.MapControllers();
app.MapHub<GPSHub>("/gpsHub");

app.Run();
