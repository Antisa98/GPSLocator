using Microsoft.AspNetCore.Mvc;
using GPSLocator.Model;
using GPSLocator.Hubs;
using Microsoft.AspNetCore.SignalR;
using GPSLocator.Services;
using GPSLocator.Models;
using System.Collections.Generic;

namespace GPSLocator.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GPSLocatorController : ControllerBase
	{
		private readonly GPSService _gpsService;
		private readonly IHubContext<GPSHub> _hubContext;

		public GPSLocatorController(GPSService gpsService, IHubContext<GPSHub> hubContext)
		{
			_gpsService = gpsService;
			_hubContext = hubContext;
		}

		[HttpGet("places")]
		public async Task<IActionResult> GetPlaces(double latitude, double longitude, int radius)
		{
			var result = await _gpsService.GetPlaces(latitude, longitude, radius);
			if (result == null)
			{
				return BadRequest("Error");
			}

			await _hubContext.Clients.All.SendAsync("ReceiveRequest", "places");

			return Ok(result);
		}

		[HttpGet("requests")]
		public async Task<ActionResult<IEnumerable<RequestModel>>> GetRequests()
		{
			return await _gpsService.GetRequests();
		}

		[HttpGet("filter")]
		public async Task<IActionResult> GetFiltered(string categoryFilter)
		{
			var result = await _gpsService.GetFilteredRequests(categoryFilter);
			return Ok(result);
		}

		[HttpGet("search")]
		public async Task<ActionResult<IEnumerable<RequestModel>>> GetRequested(string categorySearch)
		{
			var result = await _gpsService.SearchRequests(categorySearch);
			return Ok(result);
		}

		[HttpGet("registr")]
		public async Task<IActionResult> Register(string username, string password)
		{
			await _gpsService.RegisterUser(username, password);
			return Ok();
		}

		[HttpGet("login")]
		public async Task<IActionResult> Login(string username, string password)
		{
			// Add logic for login
			return Ok();
		}

		[HttpGet("favourite")]
		public async Task<IActionResult> AddToFavourite(int userId, string fsq_id)
		{
			await _gpsService.AddToFavourite(userId, fsq_id);

			return Ok();
		}

		[HttpGet("getUsers")]
		public async Task<ActionResult<IEnumerable<SimpleUser>>> GetUsers()
		{
			return await _gpsService.GetUsers();
		}
	}
}
