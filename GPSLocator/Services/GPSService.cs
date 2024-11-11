using GPSLocator.Configuration;
using GPSLocator.Data;
using GPSLocator.Handlers;
using GPSLocator.Models;
using GPSLocator.Models.Request;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GPSLocator.Services
{
	public class GPSService(GPSLocatorContext context, IHubContext<GPSHub> hubContext, UserRequestHandler handler, HttpClient httpClient, IOptions<GpsLocatorSettings> options)
	{
		private const string AUTHORIZATION_PARAMETER_TYPE_NAME = "Authorization";
		private const string RADIUS_QUERRY_PARAMETER_NAME = "radius";
		private const string LATITUDE_LONGITUDE_QUERRY_PARAMETER_NAME = "ll";

		private readonly GpsLocatorSettings settings = options.Value;

		public async Task LocateAsync(string userId, LocateRequest request)
		{
			await handler.HandleUserRequestAsync(userId, async () =>
			{
				string fullPath = settings.FoursquareApi.BaseUrl + settings.FoursquareApi.SearchPlacesPathV3;

				string path = $"{fullPath}?{LATITUDE_LONGITUDE_QUERRY_PARAMETER_NAME}={request.Latitude},{request.Longitude}&{RADIUS_QUERRY_PARAMETER_NAME}={request.Radius}";

				httpClient.DefaultRequestHeaders.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation(AUTHORIZATION_PARAMETER_TYPE_NAME, settings.FoursquareApi.ApiKey);

				var response = await httpClient.GetAsync(path);

				response.EnsureSuccessStatusCode();

				string responseContent = await response.Content.ReadAsStringAsync();

				LocationResult? result = await context.Locations.FirstOrDefaultAsync(x => x.Request == path);

				if (result == default(LocationResult))
				{
					Rootobject? responseObject = JsonSerializer.Deserialize<Rootobject>(responseContent);

					if (responseObject?.results == null)
					{
						throw new Exception("No results found in the response from Foursquare API.");
					}

					foreach (var item in responseObject.results)
					{
						if (await context.Locations.AnyAsync(x => x.Fsq_Id == item.fsq_id))
						{
							continue;
						}

						LocationResult locationResult = new LocationResult(item)
						{
							Request = path
						};

						await context.Locations.AddAsync(locationResult);
					}

					await context.SaveChangesAsync();
				}

				await hubContext.Clients.All.SendAsync("ReceiveRequest", path);
			});
		}

		public async Task<IEnumerable<LocationResult>> GetRequestsAsync(string userId)
		{
			return await handler.HandleUserRequestAsync(userId, async () =>
			{
				return await context.Locations.ToListAsync();
			});
		}

		public async Task<IEnumerable<LocationResult>> GetFilteredAsync(string userId, string categoryFilter)
		{
			return await handler.HandleUserRequestAsync(userId, async () =>
			{
				categoryFilter = categoryFilter.ToLower();

				List<LocationResult> listOfFilteredRequests = await context.Locations
					.Where(x => x.Categories
					.Any(y => y.ToLower().Equals(categoryFilter)))
					.ToListAsync();

				return listOfFilteredRequests;
			});
		}


		// Obsolite?
		public async Task<IEnumerable<LocationResult>> SearchLocationsAsync(string userId, string categorySearch)
		{
			return await handler.HandleUserRequestAsync(userId, async () =>
			{
				categorySearch = categorySearch.ToLower();

				var query = context.Locations.Where(p =>
				(!string.IsNullOrWhiteSpace(p.Fsq_Id) && p.Fsq_Id.ToLower().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Closed_Bucket) && p.Closed_Bucket.ToLower().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Distance.ToString()) && p.Distance.ToString().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Link) && p.Link.ToLower().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Name) && p.Name.ToLower().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Timezone) && p.Timezone.ToLower().Contains(categorySearch)) ||
				(!string.IsNullOrWhiteSpace(p.Request) && p.Request.ToLower().Contains(categorySearch)) ||
				(p.Categories != null && p.Categories.Any(r => !string.IsNullOrWhiteSpace(r) && r.ToLower().Contains(categorySearch))) ||
				(p.LocationInfo != null &&
				 ((!string.IsNullOrWhiteSpace(p.LocationInfo.Address) && p.LocationInfo.Address.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Country) && p.LocationInfo.Country.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Formatted_Address) && p.LocationInfo.Formatted_Address.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Locality) && p.LocationInfo.Locality.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Postcode) && p.LocationInfo.Postcode.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Region) && p.LocationInfo.Region.ToLower().Contains(categorySearch)) ||
				  (!string.IsNullOrWhiteSpace(p.LocationInfo.Cross_Street) && p.LocationInfo.Cross_Street.ToLower().Contains(categorySearch)))));

				return await query.ToListAsync();
			});
		}

		public async Task<string?> RegisterUserAsync(RegisterRequest request)
		{
			return await handler.HandleUserRequestAsync(Guid.NewGuid().ToString(), async () =>
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
			});
		}

		public async Task<string?> LoginAsync(LoginRequest request)
		{
			return await handler.HandleUserRequestAsync(Guid.NewGuid().ToString(), async () =>
			{
				User? user = await context.Users.FirstOrDefaultAsync(
				x => x.Username == request.UserName);

				if (user != default(User) && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
				{
					return user?.ApiKey;
				}

				return null;
			});
		}

		public async Task AddToFavouriteAsync(string userId, AddToFavouriteRequest request)
		{
			await handler.HandleUserRequestAsync(userId, async () =>
			{
				User? user = await context.Users.FindAsync(request.UserId);

				if (user != null && !string.IsNullOrEmpty(request.FoursquareId))
				{
					user.Favourite = request.FoursquareId;
					await context.SaveChangesAsync();
				}
			});
		}
	}
}
