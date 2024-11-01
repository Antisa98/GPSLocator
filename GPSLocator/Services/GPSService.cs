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

		public async Task LocateAsync(string UserId, LocateRequest request)
		{
			await handler.HandleUserRequestAsync(UserId, async () =>
			{
				string fullPath = settings.FoursquareApi.BaseUrl + settings.FoursquareApi.SearchPlacesPathV3;

				string path = $"{fullPath}?{LATITUDE_LONGITUDE_QUERRY_PARAMETER_NAME}={request.Latitude},{request.Longitude}&{RADIUS_QUERRY_PARAMETER_NAME}={request.Radius}";

				httpClient.DefaultRequestHeaders.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation(AUTHORIZATION_PARAMETER_TYPE_NAME, settings.FoursquareApi.ApiKey);

				var response = await httpClient.GetAsync(path);

				response.EnsureSuccessStatusCode();

				string responseContent = await response.Content.ReadAsStringAsync();

				LocationResult? result = context.Locations.FirstOrDefault(x => x.Request == path);

				if (result == default(LocationResult))
				{
					Rootobject? responseObject = JsonSerializer.Deserialize<Rootobject>(responseContent);

					if (responseObject?.results == null)
					{
						throw new Exception("No results found in the response from Foursquare API.");
					}

					foreach (var item in responseObject.results)
					{
						if (context.Locations.Any(x => x.Fsq_Id == item.fsq_id))
						{
							continue;
						}

						LocationResult locationResult = new LocationResult(item)
						{
							Request = path
						};

						context.Locations.Add(locationResult);
					}

					await context.SaveChangesAsync();
				}

				await hubContext.Clients.All.SendAsync("ReceiveRequest", path);
			});
		}

		public async Task<IEnumerable<LocationResult>> GetRequestsAsync(string UserId)
		{
			return await handler.HandleUserRequestAsync(UserId, async () =>
			{
				return await context.Locations.ToListAsync();
			});
		}

		public async Task<IEnumerable<LocationResult>> GetFilteredAsync(string UserId, string categoryFilter)
		{
			return await handler.HandleUserRequestAsync(UserId, async () =>
			{
				categoryFilter = categoryFilter.ToLower();

				List<LocationResult> listOfFilteredRequests = await context.Locations
					.Where(x => x.Categories
					.Any(y => y.ToLower().Equals(categoryFilter)))
					.ToListAsync();

				return listOfFilteredRequests;
			});
		}

		public async Task<IEnumerable<LocationResult>> SearchRequestsAsync(string UserId, string categorySearch)
		{
			return await handler.HandleUserRequestAsync(UserId, async () =>
			{
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

		public async Task AddToFavouriteAsync(string UserId, AddToFavouriteRequest request)
		{
			await handler.HandleUserRequestAsync(UserId, async () =>
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
