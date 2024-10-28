using GPSLocator.Data;
using GPSLocator.Hubs;
using GPSLocator.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Text.Json;

namespace GPSLocator.Services
{
	public class GPSService
	{
		private readonly AppDbContext _context;
		private readonly string _apiKey;
		private readonly IHubContext<GPSHub> _hubContext;


		public GPSService(AppDbContext context, IConfiguration configuration, IHubContext<GPSHub> hubContext)
		{
			_context = context;
			_apiKey = configuration["FoursquareApi:ApiKey"];
			_hubContext = hubContext;
		}

		public async Task<string> GetPlaces(string latitude_Longitude, int radius)
		{
			string querry = $"https://api.foursquare.com/v3/places/search?ll={latitude_Longitude.Replace(" ", "")}&radius={radius}";
			var options = new RestClientOptions(querry);
			var client = new RestClient(options);
			var request = new RestRequest("");
			request.AddHeader("accept", "application/json");
			request.AddHeader("Authorization", _apiKey);

			string requestString = options.BaseUrl.OriginalString;
			string responseString;

			LocationResult result = _context.Locations.FirstOrDefault(x => x.Request == requestString);

			if (result == null)
			{
				var response = await client.GetAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					return null;
				}

				responseString = response.Content;

				Rootobject? responseObject =
				JsonSerializer.Deserialize<Rootobject>(responseString);

				foreach (var item in responseObject.results)
				{
					if (_context.Locations.Any(x => x.Fsq_Id == item.fsq_id))
					{
						continue;
					}

					LocationResult locationResult = new LocationResult(item);
					locationResult.Request = requestString;
					_context.Locations.Add(locationResult);
				}
				await _context.SaveChangesAsync();
			}
			else
			{
				responseString = "That request string is already parsed in DB.";
			}

			await _hubContext.Clients.All.SendAsync("ReceiveRequest", requestString);

			return responseString;
		}

		public async Task<List<LocationResult>> GetRequests()
		{
			return await _context.Locations.ToListAsync();
		}

		public async Task<List<LocationResult>> GetFilteredRequests(string categoryFilter)
		{
			categoryFilter = categoryFilter.ToLower();

			List<LocationResult> listOfFilteredRequests = _context.Locations.Where(
				x => x.Categories.Any(
					y => y.ToLower().Equals(categoryFilter))).ToList();

			return listOfFilteredRequests;
		}

		public async Task<List<LocationResult>> SearchRequests(string categorySearch)
		{
			if (string.IsNullOrEmpty(categorySearch))
				return null;

			categorySearch = categorySearch.ToLower();
			var query = _context.Locations.AsQueryable();

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

		public async Task RegisterUser(string username, string password)
		{
			User user = new User
			{
				Username = username,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
				ApiKey = "",
				Favourite = ""
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();
		}

		public async Task AddToFavourite(int userId, string fsq_id)
		{
			User user = _context.Users.FirstOrDefault(x => x.Id == userId);

			if (user != null && !string.IsNullOrEmpty(fsq_id))
			{
				user.Favourite = fsq_id;
			}
			await _context.SaveChangesAsync();
		}

		public async Task<List<SimpleUser>> GetUsers()
		{
			return await _context.Users
				.Select(user => new SimpleUser
				{
					Id = user.Id,
					Username = user.Username
				})
				.ToListAsync();
		}
	}
}
