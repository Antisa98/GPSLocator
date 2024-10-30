namespace GPSLocator.Models.Request
{
	public record RegisterRequest
	{
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
