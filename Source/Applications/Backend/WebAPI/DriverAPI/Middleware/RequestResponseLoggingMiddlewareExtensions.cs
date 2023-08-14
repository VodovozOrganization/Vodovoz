using Microsoft.AspNetCore.Builder;

namespace DriverAPI.Middleware
{
	internal static class RequestResponseLoggingMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
		}
	}
}
