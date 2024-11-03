using GPSLocator.Models;
using Microsoft.EntityFrameworkCore;

namespace GPSLocator.Data
{
	public class GPSLocatorContext : DbContext
	{
		public GPSLocatorContext(DbContextOptions<GPSLocatorContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<LocationResult> Locations { get; set; }
	}
}
