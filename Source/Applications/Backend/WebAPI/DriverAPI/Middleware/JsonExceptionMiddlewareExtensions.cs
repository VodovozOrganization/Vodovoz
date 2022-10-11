using Microsoft.AspNetCore.Builder;

namespace DriverAPI.Middleware
{
	public static class JsonExceptionMiddlewareExtensions
	{
		public static IApplicationBuilder UseJsonExceptionsHandler(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<JsonExceptionMiddleware>();
		}
	}
}
