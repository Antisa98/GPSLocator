namespace GPSLocator.Models.Request
{
	public record LocateRequest
	{
		public required string Latitude { get; set; }
		public required string Longitude { get; set; }
		public required int Radius { get; set; }
	}
}
