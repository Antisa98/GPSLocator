using Microsoft.AspNetCore.Mvc;
using GPSLocator.Services;
using GPSLocator.Models;
using GPSLocator.Models.Request;
using GPSLocator.Filters;
using Azure.Core;
using GPSLocator.Models.Response;

namespace GPSLocator.Controllers
{
	[Route("gps-locator")]
	[ApiController]
	public class GPSLocatorController : ControllerBase
	{
		private readonly GPSService _gpsService;

		public GPSLocatorController(GPSService gpsService)
		{
			_gpsService = gpsService;
		}

		[HttpPost("places")]
		public async Task<IActionResult> LocateAsync([FromQuery] LocateRequest request)
		{
			await _gpsService.LocateAsync(request);
			return Ok();
		}

		[HttpGet("requests")]
		public async Task<ActionResult<IEnumerable<LocationResult>>> GetRequestsAsync()
		{
			return Ok(await _gpsService.GetRequestsAsync());
		}

		[HttpGet("filter")]
		public async Task<IActionResult> GetFilteredAsync([FromQuery] string categoryFilter)
		{
			return Ok(await _gpsService.GetFilteredAsync(categoryFilter));
		}

		[HttpGet("search")]
		public async Task<ActionResult<IEnumerable<LocationResult>>> SearchRequestsAsync([FromQuery] string categorySearch)
		{
			return Ok(await _gpsService.SearchRequestsAsync(categorySearch));
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync(RegisterRequest request)
		{
			await _gpsService.RegisterUserAsync(request);
			return Ok();
		}

		[HttpPost("login")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
		public async Task<IActionResult> LoginAsync(LoginRequest request)
		{

			var result = await _gpsService.LoginAsync(request);

			if (result == null)
			{
				return Unauthorized("Invalid username or password.");
			}

			return Ok(new LoginResponse { ApiKey = result });
		}

		[HttpPost("favourite")]
		[APIKeyFilter]
		public async Task<IActionResult> AddToFavouriteAsync(AddToFavouriteRequest request)
		{
			await _gpsService.AddToFavouriteAsync(request);

			return Ok();
		}
	}
}
