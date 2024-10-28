using GPSLocator.Models;
using Microsoft.EntityFrameworkCore;

namespace GPSLocator.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		
		public DbSet<LocationResult> Locations { get; set; }
	}
}
