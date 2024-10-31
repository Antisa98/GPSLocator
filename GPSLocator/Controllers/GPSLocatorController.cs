﻿using GPSLocator.Filters;
using GPSLocator.Models;
using GPSLocator.Models.Request;
using GPSLocator.Models.Response;
using GPSLocator.Services;
using Microsoft.AspNetCore.Mvc;

namespace GPSLocator.Controllers
{
	[Route("gps-locator")]
	[ApiController]
	public class GPSLocatorController(GPSService gpsService) : ControllerBase
	{
		[HttpPost("places")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<IActionResult> LocateAsync([FromQuery] LocateRequest request)
		{
			await gpsService.LocateAsync(request);
			return Ok();
		}

		[HttpGet("requests")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<ActionResult<IEnumerable<LocationResult>>> GetRequestsAsync()
		{
			return Ok(await gpsService.GetRequestsAsync());
		}

		[HttpGet("filter")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<IActionResult> GetFilteredAsync([FromQuery] string categoryFilter)
		{
			return Ok(await gpsService.GetFilteredAsync(categoryFilter));
		}

		[HttpGet("search")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<ActionResult<IEnumerable<LocationResult>>> SearchRequestsAsync([FromQuery] string categorySearch)
		{
			return Ok(await gpsService.SearchRequestsAsync(categorySearch));
		}

		[HttpPost("register")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
		public async Task<IActionResult> RegisterAsync(RegisterRequest request)
		{
			var result = await gpsService.RegisterUserAsync(request);

			if (result == null)
			{
				return Unauthorized("Username already exists.");
			}

			return Ok(new RegisterResponse { ApiKey = result });
		}

		[HttpPost("login")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
		public async Task<IActionResult> LoginAsync(LoginRequest request)
		{
			var result = await gpsService.LoginAsync(request);

			if (result == null)
			{
				return Unauthorized("Invalid username or password.");
			}

			return Ok(new LoginResponse { ApiKey = result });
		}

		[HttpPost("favourite")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<IActionResult> AddToFavouriteAsync(AddToFavouriteRequest request)
		{
			await gpsService.AddToFavouriteAsync(request);

			return Ok();
		}
	}
}
