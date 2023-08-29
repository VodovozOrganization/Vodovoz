using Microsoft.AspNetCore.Builder;

namespace DriverAPI.Middleware
{
	internal static class JsonExceptionMiddlewareExtensions
	{
		public static IApplicationBuilder UseJsonExceptionsHandler(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<JsonExceptionMiddleware>();
		}
	}
}
