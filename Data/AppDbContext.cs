using GPSLocator.Model;
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

		public DbSet<RequestModel> Requests { get; set; }
		public DbSet<User> Users { get; set; }
	}
}
