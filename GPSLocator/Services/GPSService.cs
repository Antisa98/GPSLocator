using GPSLocator.Data;
using GPSLocator.Hubs;
using GPSLocator.Models;
using GPSLocator.Models.Request;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace GPSLocator.Services
{
	public class GPSService(GPSLocatorContext context, IConfiguration configuration, IHubContext<GPSHub> hubContext, HttpClient httpClient)
	{
		private const string AUTHORIZATION_PARAMETER_TYPE_NAME = "Authorization";

		public async Task LocateAsync(LocateRequest request)
		{
			string apiKey = configuration["FoursquareApi:ApiKey"];
			string path = $"https://api.foursquare.com/v3/places/search?ll={request.Latitude},{request.Longitude}&radius={request.Radius}";

			//httpClient.DefaultRequestHeaders.Clear();
			//httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			//httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTHORIZATION_PARAMETER_TYPE_NAME, apiKey);


			var options = new RestClientOptions(path);
			var client = new RestClient(options);
			var restRequest = new RestRequest("");
			restRequest.AddHeader("accept", "application/json");
			restRequest.AddHeader(AUTHORIZATION_PARAMETER_TYPE_NAME, apiKey);
			var response = await client.GetAsync(restRequest);



			LocationResult result = context.Locations.FirstOrDefault(x => x.Request == path);

			if (result == null)
			{
				//var response = await httpClient.GetAsync(path);
				//response.EnsureSuccessStatusCode();
				//string responseString = await response.Content.ReadAsStringAsync();

				Rootobject? responseObject =
				JsonSerializer.Deserialize<Rootobject>(/*responseString*/response.Content);

				foreach (var item in responseObject.results)
				{
					if (context.Locations.Any(x => x.Fsq_Id == item.fsq_id))
					{
						continue;
					}

					LocationResult locationResult = new LocationResult(item);
					locationResult.Request = path;
					context.Locations.Add(locationResult);
				}

				await context.SaveChangesAsync();
			}

			await hubContext.Clients.All.SendAsync("ReceiveRequest", path);
		}

		public async Task<List<LocationResult>> GetRequestsAsync()
		{
			return await context.Locations.ToListAsync();
		}

		public async Task<List<LocationResult>> GetFilteredAsync(string categoryFilter)
		{
			categoryFilter = categoryFilter.ToLower();

			List<LocationResult> listOfFilteredRequests = context.Locations.Where(
				x => x.Categories.Any(
					y => y.ToLower().Equals(categoryFilter))).ToList();

			return listOfFilteredRequests;
		}

		public async Task<List<LocationResult>> SearchRequestsAsync(string categorySearch)
		{
			if (string.IsNullOrEmpty(categorySearch))
				return null;

			categorySearch = categorySearch.ToLower();
			var query = context.Locations.AsQueryable();

			query = query.Where(p =>
				p.Fsq_Id.Contains(categorySearch) ||
				p.Closed_Bucket.Contains(categorySearch) ||
				p.Distance.ToString().Contains(categorySearch) ||
				p.Link.Contains(categorySearch) ||
				p.Name.Contains(categorySearch) ||
				p.Timezone.Contains(categorySearch) ||
				p.Request.Contains(categorySearch) ||
				p.Categories.Any(r => r.Contains(categorySearch)) ||
				(p.LocationInfo != null &&
				(p.LocationInfo.Address != null && p.LocationInfo.Address.Contains(categorySearch) ||
				 p.LocationInfo.Country != null && p.LocationInfo.Country.Contains(categorySearch) ||
				 p.LocationInfo.Formatted_Address != null && p.LocationInfo.Formatted_Address.Contains(categorySearch) ||
				 p.LocationInfo.Locality != null && p.LocationInfo.Locality.Contains(categorySearch) ||
				 p.LocationInfo.Postcode != null && p.LocationInfo.Postcode.Contains(categorySearch) ||
				 p.LocationInfo.Region != null && p.LocationInfo.Region.Contains(categorySearch) ||
				 p.LocationInfo.Cross_Street != null && p.LocationInfo.Cross_Street.Contains(categorySearch))));

			return await query.ToListAsync();
		}

		public async Task RegisterUserAsync(RegisterRequest request)
		{
			User? user = await context.Users.FirstOrDefaultAsync(x => x.Username == request.UserName);

			if (user != default(User))
			{
				return;
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
		}

		public async Task<string> LoginAsync(LoginRequest request)
		{
			User? user = await context.Users.FirstOrDefaultAsync(
				x => x.Username == request.UserName);

			if (user != default(User) &&
				BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return user.ApiKey;
			}

			return "";
		}

		public async Task AddToFavouriteAsync(AddToFavouriteRequest request)
		{
			User? user = await context.Users.FindAsync(request.UserId);

			if (user != null && !string.IsNullOrEmpty(request.FoursquareId))
			{
				user.Favourite = request.FoursquareId;
			}

			await context.SaveChangesAsync();
		}
	}
}
