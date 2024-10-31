using GPSLocator.Data;
using GPSLocator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace GPSLocator.Filters
{
	public class APIKeyFilter(GPSLocatorContext gPSLocatorContext) : ActionFilterAttribute
	{
		private const string APIKeyHeaderName = "X-Api-Key";
		private const string UserIdContextKey = "UserId";

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			context.HttpContext.Request.Headers.TryGetValue(APIKeyHeaderName, out StringValues headerValue);

			User? user = gPSLocatorContext.Users.FirstOrDefault(x => x.ApiKey == headerValue.ToString());
			if (user == default(User))
			{
				context.Result = new UnauthorizedResult(); // Set result to stop action execution
			}
			else
			{
				context.HttpContext.Items[UserIdContextKey] = user.Id;
			}
		}
	}
}
