using System.ComponentModel.DataAnnotations;

namespace GPSLocator.Model
{
	public class RequestModel
	{
		[Key]
		public int Id { get; set; }
		public string Request { get; set; }
		public string Response { get; set; }
	}
}
