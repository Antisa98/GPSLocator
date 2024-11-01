using GPSLocator.Configuration;
using GPSLocator.Data;
using GPSLocator.Filters;
using GPSLocator.Handlers;
using GPSLocator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GPSLocator.Composition
{
	public static class CompositionRoot
	{
		public static IServiceCollection RegisterAplicationDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			services.RegisterSettings(configuration)
					.RegisterInfrastructureServices()
					.RegisterDatabaseContext(configuration);

			return services;
		}

		private static IServiceCollection RegisterSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<GpsLocatorSettings>(options => configuration.GetSection(nameof(GpsLocatorSettings)).Bind(options));

			return services;
		}

		private static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
		{
			services.AddHttpClient<GPSService>();
			services.AddSignalR();
			services.AddScoped<APIKeyFilter>();
			services.AddScoped<GPSService>();
			services.AddScoped<UserRequestHandler>();

			return services;
		}

		private static IServiceCollection RegisterDatabaseContext(this IServiceCollection services, IConfiguration configuration)
		{
			ServiceProvider serviceProvider = services.BuildServiceProvider();

			using IServiceScope scope = serviceProvider.CreateScope();

			var settings = scope.ServiceProvider.GetService<IOptions<GpsLocatorSettings>>().Value;

			services.AddDbContext<GPSLocatorContext>(options =>
				options.UseSqlServer(settings.DatabaseSettings.ConnectionString));

			if (string.IsNullOrEmpty(settings.FoursquareApi.ApiKey))
			{
				throw new Exception($"Missing credentials for {nameof(GpsLocatorSettings.FoursquareApi)}.");
			}

			return services;
		}

	}
}
