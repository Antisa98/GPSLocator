using GPSLocator.Data;
using GPSLocator.Model;
using GPSLocator.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Collections.Generic;
using System.Text.Json;
using static GPSLocator.Models.Rootobject;

namespace GPSLocator.Services
{
	public class GPSService
	{
		private readonly AppDbContext _context;
		private readonly string _apiKey;

		public GPSService(AppDbContext context, IConfiguration configuration)
		{
			_context = context;
			_apiKey = configuration["FoursquareApi:ApiKey"];
		}

		public async Task<string> GetPlaces(double latitude, double longitude, int radius)
		{
			string querry = $"https://api.foursquare.com/v3/places/search?ll={latitude.ToString().Replace(',', '.')}%2C{longitude.ToString().Replace(',', '.')}&radius={radius}";
			var options = new RestClientOptions(querry);
			var client = new RestClient(options);
			var request = new RestRequest("");
			request.AddHeader("accept", "application/json");
			request.AddHeader("Authorization", _apiKey);

			string requestString = options.BaseUrl.OriginalString;
			string responseString;

			RequestModel rm = _context.Requests.FirstOrDefault(x => x.Request == requestString);

			if (rm == null)
			{
				var response = await client.GetAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					return null;
				}

				responseString = response.Content;

				RequestModel requestModel = new RequestModel { Request = requestString, Response = responseString };
				_context.Requests.Add(requestModel);
				_context.SaveChanges();
			}
			else
			{
				responseString = rm.Response;
			}

			return responseString;
		}

		public async Task<List<RequestModel>> GetRequests()
		{
			return await _context.Requests.ToListAsync();
		}

		public async Task<List<Result>> GetFilteredRequests(string categoryFilter)
		{
			List<RequestModel> listOfRequests = await _context.Requests.ToListAsync();
			if (listOfRequests == null)
			{
				return null;
			}

			List<Rootobject> rootobjects = listOfRequests.Select(item => JsonSerializer.Deserialize<Rootobject>(item.Response)).ToList();

			categoryFilter = categoryFilter.ToLower();
			List<Result> listOfFilteredRequests = new List<Result>();

			foreach (var item in rootobjects)
			{
				var filteredResults = item.results
					.Where(result => result.categories != null &&
									 result.categories.Any(category => category.name.ToLower() == categoryFilter ||
									 category.plural_name.ToLower() == categoryFilter ||
									 category.short_name.ToLower() == categoryFilter))
					.ToList();

				listOfFilteredRequests.AddRange(filteredResults);
			}

			return listOfFilteredRequests;
		}

		// TODO
		public async Task<List<RequestModel>> SearchRequests(string categorySearch)
		{
			List<RequestModel> listOfRequests = await _context.Requests.ToListAsync();
			if (listOfRequests == null)
			{
				return null;
			}

			categorySearch = categorySearch.ToLower();

			foreach (RequestModel item in listOfRequests)
			{
				string str = item.Response.ToLower();
				if (String.IsNullOrEmpty(categorySearch))
					throw new ArgumentException("The string to find may not be empty", nameof(categorySearch));

				var indexes = new List<int>();
				for (int index = 0; ; index += categorySearch.Length)
				{
					index = str.IndexOf(categorySearch, index);
					if (index == -1)
						break;
					indexes.Add(index);
				}

				Rootobject re = JsonSerializer.Deserialize<Rootobject>(item.Response);
			}

			return listOfRequests;
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
			_context.SaveChanges();
		}

		public async Task AddToFavourite(int userId, string fsq_id)
		{
			User user = _context.Users.FirstOrDefault(x => x.Id == userId);

			if (user != null && !string.IsNullOrEmpty(fsq_id))
			{
				user.Favourite = fsq_id;
			}
			_context.SaveChanges();
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
