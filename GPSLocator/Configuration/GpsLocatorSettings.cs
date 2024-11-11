namespace GPSLocator.Configuration
{
	public record GpsLocatorSettings
	{
		public required FoursquareApiClientElement FoursquareApi { get; set; }
		public required DatabaseSettingsElement DatabaseSettings { get; set; }
	}
}
