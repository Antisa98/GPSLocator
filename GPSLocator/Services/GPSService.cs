using GPSLocator.Data;
using GPSLocator.Hubs;
using GPSLocator.Models;
using GPSLocator.Models.Request;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Text.Json;

namespace GPSLocator.Services
{
	public class GPSService(GPSLocatorContext context, IConfiguration configuration, IHubContext<GPSHub> hubContext, HttpClient httpClient)
	{
		private const string AUTHORIZATION_PARAMETER_TYPE_NAME = "Authorization";

		//public async Task LocateAsync(LocateRequest request)
		//{
		//	string apiKey = configuration["FoursquareApi:ApiKey"];
		//	string path = $"https://api.foursquare.com/v3/places/search?ll={request.Latitude},{request.Longitude}&radius={request.Radius}";

		//	//httpClient.DefaultRequestHeaders.Clear();
		//	//httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		//	//httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTHORIZATION_PARAMETER_TYPE_NAME, apiKey);

		//	var options = new RestClientOptions(path);
		//	var client = new RestClient(options);
		//	var restRequest = new RestRequest("");
		//	restRequest.AddHeader("accept", "application/json");
		//	restRequest.AddHeader(AUTHORIZATION_PARAMETER_TYPE_NAME, apiKey);
		//	var response = await client.GetAsync(restRequest);

		//	if (!response.IsSuccessful || response.Content == null)
		//	{
		//		throw new HttpRequestException($"Error fetching data from Foursquare API: {response.StatusCode} - {response.ErrorMessage}");
		//	}

		//	LocationResult? result = context.Locations.FirstOrDefault(x => x.Request == path);

		//	if (result == default(LocationResult))
		//	{
		//		//var response = await httpClient.GetAsync(path);
		//		//response.EnsureSuccessStatusCode();
		//		//string responseString = await response.Content.ReadAsStringAsync();

		//		Rootobject? responseObject =
		//		JsonSerializer.Deserialize<Rootobject>(/*responseString*/response.Content);

		//		foreach (var item in responseObject.results)
		//		{
		//			if (context.Locations.Any(x => x.Fsq_Id == item.fsq_id))
		//			{
		//				continue;
		//			}

		//			LocationResult locationResult = new LocationResult(item);
		//			locationResult.Request = path;
		//			context.Locations.Add(locationResult);
		//		}

		//		await context.SaveChangesAsync();
		//	}

		//	await hubContext.Clients.All.SendAsync("ReceiveRequest", path);
		//}

		public async Task LocateAsync(LocateRequest request)
		{
			string apiKey = configuration["FoursquareApi:ApiKey"];
			string path = $"https://api.foursquare.com/v3/places/search?ll={request.Latitude},{request.Longitude}&radius={request.Radius}";

			var options = new RestClientOptions(path);
			var client = new RestClient(options);
			var restRequest = new RestRequest("");
			restRequest.AddHeader("accept", "application/json");
			restRequest.AddHeader(AUTHORIZATION_PARAMETER_TYPE_NAME, apiKey);

			var response = await client.GetAsync(restRequest);

			// Check if the response was successful; if not, throw an exception
			if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
			{
				throw new HttpRequestException($"Error fetching data from Foursquare API: {response.StatusCode} - {response.ErrorMessage}");
			}

			// Check if the location has already been stored in the context
			LocationResult? result = context.Locations.FirstOrDefault(x => x.Request == path);

			if (result == default(LocationResult))
			{
				// Deserialize the JSON response into a Rootobject instance
				Rootobject? responseObject = JsonSerializer.Deserialize<Rootobject>(response.Content);

				if (responseObject?.results == null) // Handle potential null results
				{
					throw new Exception("No results found in the response from Foursquare API.");
				}

				foreach (var item in responseObject.results)
				{
					// Check if the location already exists in the context
					if (context.Locations.Any(x => x.Fsq_Id == item.fsq_id))
					{
						continue; // Skip if the location already exists
					}

					// Create a new LocationResult instance
					LocationResult locationResult = new LocationResult(item)
					{
						Request = path // Set the request path for the location
					};

					// Add the new location to the context
					context.Locations.Add(locationResult);
				}

				// Save changes to the database
				await context.SaveChangesAsync();
			}

			// Notify all connected clients about the new request
			await hubContext.Clients.All.SendAsync("ReceiveRequest", path);
		}

		public async Task<IEnumerable<LocationResult>> GetRequestsAsync()
		{
			return await context.Locations.ToListAsync();
		}

		public async Task<IEnumerable<LocationResult>> GetFilteredAsync(string categoryFilter)
		{
			categoryFilter = categoryFilter.ToLower();

			List<LocationResult> listOfFilteredRequests = await context.Locations
				.Where(x => x.Categories.Any(y =>
					string.Equals(y, categoryFilter, StringComparison.OrdinalIgnoreCase)))
				.ToListAsync();

			return listOfFilteredRequests;
		}

		public async Task<IEnumerable<LocationResult>> SearchRequestsAsync(string categorySearch)
		{
			if (string.IsNullOrEmpty(categorySearch))
				return Enumerable.Empty<LocationResult>(); // Return an empty list instead of null

			categorySearch = categorySearch.ToLower();
			var query = context.Locations.AsQueryable();

			query = query.Where(p =>
				(p.Fsq_Id != null && p.Fsq_Id.ToLower().Contains(categorySearch)) ||
				(p.Closed_Bucket != null && p.Closed_Bucket.ToLower().Contains(categorySearch)) ||
				p.Distance.ToString().Contains(categorySearch) ||
				(p.Link != null && p.Link.ToLower().Contains(categorySearch)) ||
				(p.Name != null && p.Name.ToLower().Contains(categorySearch)) ||
				(p.Timezone != null && p.Timezone.ToLower().Contains(categorySearch)) ||
				(p.Request != null && p.Request.ToLower().Contains(categorySearch)) ||
				p.Categories.Any(r => r.ToLower().Contains(categorySearch)) ||
				(p.LocationInfo != null &&
				 ((p.LocationInfo.Address != null && p.LocationInfo.Address.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Country != null && p.LocationInfo.Country.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Formatted_Address != null && p.LocationInfo.Formatted_Address.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Locality != null && p.LocationInfo.Locality.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Postcode != null && p.LocationInfo.Postcode.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Region != null && p.LocationInfo.Region.ToLower().Contains(categorySearch)) ||
				  (p.LocationInfo.Cross_Street != null && p.LocationInfo.Cross_Street.ToLower().Contains(categorySearch))))
			);

			return await query.ToListAsync();
		}


		public async Task<string?> RegisterUserAsync(RegisterRequest request)
		{
			User? user = await context.Users.FirstOrDefaultAsync(x => x.Username == request.UserName);

			if (user != default(User))
			{
				return null;
			}

			user = new User
			{
				Username = request.UserName,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
				ApiKey = Guid.NewGuid().ToString(),
				Favourite = ""
			};

			context.Users.Add(user);
			await context.SaveChangesAsync();

			return user.ApiKey;
		}

		public async Task<string?> LoginAsync(LoginRequest request)
		{
			User? user = await context.Users.FirstOrDefaultAsync(
				x => x.Username == request.UserName);

			return user?.ApiKey;
		}

		public async Task AddToFavouriteAsync(AddToFavouriteRequest request)
		{
			User? user = await context.Users.FindAsync(request.UserId);

			if (user != null && !string.IsNullOrEmpty(request.FoursquareId))
			{
				user.Favourite = request.FoursquareId;
				await context.SaveChangesAsync();
			}
		}
	}
}
