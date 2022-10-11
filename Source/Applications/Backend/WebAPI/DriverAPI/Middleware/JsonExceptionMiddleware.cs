using DriverAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DriverAPI.Middleware
{
	public class JsonExceptionMiddleware
	{
		private readonly ILogger<JsonExceptionMiddleware> _logger;
		private readonly RequestDelegate _next;

		public JsonExceptionMiddleware(ILogger<JsonExceptionMiddleware> logger, RequestDelegate next)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_next = next ?? throw new ArgumentNullException(nameof(next));
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch(Exception exception)
			{
				var code = StatusCodes.Status500InternalServerError;

				_logger.LogError(exception, exception.Message);

				if(exception is UnauthorizedAccessException)
				{
					code = StatusCodes.Status401Unauthorized;
				}

				context.Response.StatusCode = code;
				context.Response.ContentType = "application/json; charset=utf-8";

				await context.Response.Body.FlushAsync();

				await context.Response.Body
					.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer
						.Serialize(new ErrorResponseDto(exception.Message))));
			}
		}
	}
}
