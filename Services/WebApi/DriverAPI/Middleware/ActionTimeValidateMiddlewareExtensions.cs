using Microsoft.AspNetCore.Builder;

namespace DriverAPI.Middleware
{
	public static class ActionTimeValidateMiddlewareExtensions
	{
		public static IApplicationBuilder UseActionTimeValidation(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ActionTimeValidateMiddleware>();
		}
	}
}
