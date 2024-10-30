using GPSLocator.Data;
using GPSLocator.Filters;
using GPSLocator.Hubs;
using GPSLocator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GPSService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<APIKeyFilter>();
builder.Services.AddScoped<GPSService>();

// Register the database context
builder.Services.AddDbContext<GPSLocatorContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
