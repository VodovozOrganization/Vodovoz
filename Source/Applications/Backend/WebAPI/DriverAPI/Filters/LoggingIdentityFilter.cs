using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace DriverAPI.Filters
{
	/// <summary>
	/// Logs the identity of the user making the request.
	/// </summary>
	public class LoggingIdentityFilter : IAsyncActionFilter
	{
		private readonly ILogger<LoggingIdentityFilter> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingIdentityFilter"/> class.
		/// </summary>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public LoggingIdentityFilter(ILogger<LoggingIdentityFilter> logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Logs the identity of the user making the request.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="next"></param>
		/// <returns></returns>
		public async Task OnActionExecutionAsync(
			ActionExecutingContext context,
			ActionExecutionDelegate next)
		{
			var username = context.HttpContext.User.Identity?.Name ?? "Unknown";
			var accessToken = context.HttpContext.Request.Headers[HeaderNames.Authorization];
			var uri = context.HttpContext.Request.Path;

			_logger.LogInformation(
				"User {Username} with token {AccessToken} is trying to access {Uri}",
				username,
				accessToken,
				uri);

			await next.Invoke();
		}
	}
}
