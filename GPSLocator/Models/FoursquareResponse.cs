namespace GPSLocator.Models
{
	public class Rootobject
	{
		public Result[] results { get; set; }
		public Context context { get; set; }

		public class Context
		{
			public Geo_Bounds geo_bounds { get; set; }
		}

		public class Geo_Bounds
		{
			public Circle circle { get; set; }
		}

		public class Circle
		{
			public Center center { get; set; }
			public int radius { get; set; }
		}

		public class Center
		{
			public float latitude { get; set; }
			public float longitude { get; set; }
		}

		public class Result
		{
			public string fsq_id { get; set; }
			public Category1[] categories { get; set; }
			public object[] chains { get; set; }
			public string closed_bucket { get; set; }
			public int distance { get; set; }
			public Geocodes geocodes { get; set; }
			public string link { get; set; }
			public Location location { get; set; }
			public string name { get; set; }
			public Related_Places related_places { get; set; }
			public string timezone { get; set; }
		}

		public class Geocodes
		{
			public Main main { get; set; }
			public Roof roof { get; set; }
			public Drop_Off drop_off { get; set; }
		}

		public class Main
		{
			public float latitude { get; set; }
			public float longitude { get; set; }
		}

		public class Roof
		{
			public float latitude { get; set; }
			public float longitude { get; set; }
		}

		public class Drop_Off
		{
			public float latitude { get; set; }
			public float longitude { get; set; }
		}

		public class Location
		{
			public string address { get; set; }
			public string country { get; set; }
			public string formatted_address { get; set; }
			public string locality { get; set; }
			public string postcode { get; set; }
			public string region { get; set; }
			public string cross_street { get; set; }
		}

		public class Related_Places
		{
			public Child[] children { get; set; }
		}

		public class Child
		{
			public string fsq_id { get; set; }
			public Category[] categories { get; set; }
			public string name { get; set; }
		}

		public class Category
		{
			public int id { get; set; }
			public string name { get; set; }
			public string short_name { get; set; }
			public string plural_name { get; set; }
			public Icon icon { get; set; }
		}

		public class Icon
		{
			public string prefix { get; set; }
			public string suffix { get; set; }
		}

		public class Category1
		{
			public int id { get; set; }
			public string name { get; set; }
			public string short_name { get; set; }
			public string plural_name { get; set; }
			public Icon1 icon { get; set; }
		}

		public class Icon1
		{
			public string prefix { get; set; }
			public string suffix { get; set; }
		}

	}


}
