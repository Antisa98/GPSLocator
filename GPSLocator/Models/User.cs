using System.ComponentModel.DataAnnotations;

namespace GPSLocator.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		public string? Username { get; set; }
		public string? PasswordHash { get; set; }
		public string? ApiKey { get; set; }
		public string? Favourite { get; set; }
	}
}
