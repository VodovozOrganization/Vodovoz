using Microsoft.AspNetCore.Builder;

namespace MailganerEventsDistributorApi.Middleware
{
	public static class RequestResponseLoggingMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
		}
	}
}
