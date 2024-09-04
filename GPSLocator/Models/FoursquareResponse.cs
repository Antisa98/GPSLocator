using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPSLocator.Models
{
	public class Rootobject
	{
		[Key]
		public int Id { get; set; }
		public virtual ICollection<Result> Results { get; set; } // Using ICollection for one-to-many relationship
		public int ContextId { get; set; }
		[JsonProperty(PropertyName = "context")]
		public virtual Context Context { get; set; }
	}

	public class Context
	{
		[Key]
		public int Id { get; set; }
		public int GeoBoundsId { get; set; }
		public virtual GeoBounds Geo_Bounds { get; set; }
	}

	public class GeoBounds
	{
		[Key]
		public int Id { get; set; }
		public int CircleId { get; set; }
		public virtual Circle Circle { get; set; }
	}

	public class Circle
	{
		[Key]
		public int Id { get; set; }
		public int CenterId { get; set; }
		public virtual Center Center { get; set; }
		public int Radius { get; set; }
	}

	public class Center
	{
		[Key]
		public int Id { get; set; }
		public float Latitude { get; set; }
		public float Longitude { get; set; }
	}

	public class Result
	{
		[Key]
		public int Id { get; set; }
		public string? Fsq_Id { get; set; }
		public virtual ICollection<Category1> Categories { get; set; } 
		public string? Closed_Bucket { get; set; }
		public int Distance { get; set; }
		[NotMapped]
		public int GeocodesId { get; set; }
		[NotMapped]
		public virtual Geocodes Geocodes { get; set; }
		public string? Link { get; set; }
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }
		public string? Name { get; set; }
		[NotMapped]
		public int RelatedPlacesId { get; set; }
		[NotMapped]
		public virtual RelatedPlaces RelatedPlaces { get; set; }
		public string? Timezone { get; set; }
	}

	public class Geocodes
	{
		[Key]
		public int Id { get; set; }
		public int MainId { get; set; }
		public virtual Main Main { get; set; }
		public int RoofId { get; set; }
		public virtual Roof Roof { get; set; }
	}

	public class Main
	{
		[Key]
		public int Id { get; set; }
		public float Latitude { get; set; }
		public float Longitude { get; set; }
	}

	public class Roof
	{
		[Key]
		public int Id { get; set; }
		public float Latitude { get; set; }
		public float Longitude { get; set; }
	}

	public class DropOff
	{
		[Key]
		public int Id { get; set; }
		public float Latitude { get; set; }
		public float Longitude { get; set; }
	}

	public class Location
	{
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

	public class RelatedPlaces
	{
		[Key]
		public int Id { get; set; }
		public virtual ICollection<Child> Children { get; set; }
	}

	public class Child
	{
		[Key]
		public int Id { get; set; }
		public string? Fsq_Id { get; set; }
		public virtual ICollection<Category> Categories { get; set; }
		public string? Name { get; set; }
	}

	public class Category
	{
		[Key]
		public int Id { get; set; }
		public int ExternalId { get; set; }
		public string? Name { get; set; }
		public string? Short_Name { get; set; }
		public string? Plural_Name { get; set; }
		public int IconId { get; set; }
		public virtual Icon Icon { get; set; }
	}

	public class Icon
	{
		[Key]
		public int Id { get; set; }
		public string? Prefix { get; set; }
		public string? Suffix { get; set; }
	}

	public class Category1
	{
		[Key]
		[Column("Id")]
		public int DbId { get; set; }

		[JsonProperty("id")]
		public int ExternalId { get; set; }
		public string? Name { get; set; }
		public string? Short_Name { get; set; }
		public string? Plural_Name { get; set; }
		[NotMapped]
		public int IconId { get; set; }
		[NotMapped]
		public virtual Icon1 Icon { get; set; }
	}

	public class Icon1
	{
		[Key]
		public int Id { get; set; }
		public string? Prefix { get; set; }
		public string? Suffix { get; set; }
	}
}
