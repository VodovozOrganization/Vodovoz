using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Sms.Internal.Service.Middleware
{
	public class ApiKeyMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;
		private const string _apiKeyParameterName = "ApiKey";

		public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
		{
			_next = next;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if(!context.Request.Headers.TryGetValue(_apiKeyParameterName, out var apiKeyFromRequest))
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("Api Key was not provided.");
				return;
			}

			var apiKey = _configuration.GetValue<string>(_apiKeyParameterName);

			if(!apiKeyFromRequest.Equals(apiKey))
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("Unauthorized client.");
				return;
			}

			await _next(context);
		}
	}
}
