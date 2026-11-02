using Microsoft.AspNetCore.Builder;

namespace DriverAPI.Middleware
{
	internal static class RequestResponseLoggingMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
		}

		/// <summary>
		/// Должен быть зарегистрирован после <c>UseAuthentication</c>, т.к. лимит считается по пользователю
		/// </summary>
		public static IApplicationBuilder UsePerUserConcurrencyLimit(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<PerUserConcurrencyLimitMiddleware>();
		}
	}
}
