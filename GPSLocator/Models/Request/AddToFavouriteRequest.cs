using System.Text.Json.Serialization;

namespace GPSLocator.Models.Request
{
	public record AddToFavouriteRequest
	{
		public int UserId { get; set; }

		[JsonPropertyName("fsq_id")]
		public string FoursquareId { get; set; }
	}
}
