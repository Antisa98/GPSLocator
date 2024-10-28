using System.ComponentModel.DataAnnotations;

namespace GPSLocator.Models
{
	public class LocationResult
	{
        public LocationResult()
        {
        }
        public LocationResult(Result result)
		{
			this.Fsq_Id = result.fsq_id;
			this.Closed_Bucket = result.closed_bucket;
			this.Distance = result.distance;
			this.Link = result.link;
			this.Name = result.name;
			this.Timezone = result.timezone;
			this.Categories = new List<string>();

			foreach (var item in result.categories)
			{
				if (!string.IsNullOrEmpty(item.plural_name)) this.Categories.Add(item.plural_name);
				if (!string.IsNullOrEmpty(item.name)) this.Categories.Add(item.name);
				if (!string.IsNullOrEmpty(item.short_name)) this.Categories.Add(item.short_name);
			}

			this.LocationInfo = new LocationInfo(result.location);
		}

		[Key]
		public int Id { get; set; }
		public string? Fsq_Id { get; set; }
		public virtual List<string> Categories { get; set; }
		public string? Closed_Bucket { get; set; }
		public int Distance { get; set; }
		public string? Link { get; set; }
		public virtual LocationInfo LocationInfo { get; set; }
		public string? Name { get; set; }
		public string? Timezone { get; set; }
		public string? Request { get; set; }
	}

	public class LocationInfo
	{
        public LocationInfo()
        {
            
        }
        public LocationInfo(Location location)
		{
			this.Address = location.address;
			this.Country = location.country;
			this.Formatted_Address = location.formatted_address;
			this.Locality = location.locality;
			this.Postcode = location.postcode;
			this.Region = location.region;
			this.Cross_Street = location.cross_street;
		}

		[Key]
		public int Id { get; set; }
		public string? Address { get; set; }
		public string? Country { get; set; }
		public string? Formatted_Address { get; set; }
		public string? Locality { get; set; }
		public string? Postcode { get; set; }
		public string? Region { get; set; }
		public string? Cross_Street { get; set; }
	}
}
