namespace GPSLocator.Configuration
{
	public record FoursquareApiClientElement
	{
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
        public required string SearchPlacesPathV3 { get; set; }
	}
}
