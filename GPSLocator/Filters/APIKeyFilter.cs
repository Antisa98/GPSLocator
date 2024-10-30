using GPSLocator.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace GPSLocator.Filters
{
	public class APIKeyFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			const string APIKeyHeaderName = "X-Api-Key";
			if (!context.HttpContext.Request.Headers.TryGetValue(APIKeyHeaderName, out StringValues headerValue))
			{
				throw new HttpRequestException();
			}
		}
	}
}
