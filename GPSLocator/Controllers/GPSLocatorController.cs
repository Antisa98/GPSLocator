using GPSLocator.Filters;
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
			string userId = HttpContext.Items["UserId"].ToString();
			await gpsService.LocateAsync(userId, request);

			return Ok();
		}

		[HttpGet("requests")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<ActionResult<IEnumerable<LocationResult>>> GetRequestsAsync()
		{
			string userId = HttpContext.Items["UserId"].ToString();

			return Ok(await gpsService.GetRequestsAsync(userId));
		}

		[HttpGet("filter")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<IActionResult> GetFilteredAsync([FromQuery] string categoryFilter)
		{
			string userId = HttpContext.Items["UserId"].ToString();

			return Ok(await gpsService.GetFilteredAsync(userId, categoryFilter));
		}

		[HttpGet("search")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<ActionResult<IEnumerable<LocationResult>>> SearchLocationsAsync([FromQuery] string categorySearch)
		{
			string userId = HttpContext.Items["UserId"].ToString();

			return Ok(await gpsService.SearchLocationsAsync(userId, categorySearch));
		}

		[HttpPost("register")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
		public async Task<IActionResult> RegisterAsync(RegisterRequest request)
		{
			var result = await gpsService.RegisterUserAsync(request);

			if (result == null)
			{
				return BadRequest("Username already exists.");
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
				return BadRequest("Invalid username or password.");
			}

			return Ok(new LoginResponse { ApiKey = result });
		}

		[HttpPost("favourite")]
		[ServiceFilter(typeof(APIKeyFilter))]
		public async Task<IActionResult> AddToFavouriteAsync(AddToFavouriteRequest request)
		{
			string userId = HttpContext.Items["UserId"].ToString();

			await gpsService.AddToFavouriteAsync(userId, request);
			return Ok();
		}
	}
}
