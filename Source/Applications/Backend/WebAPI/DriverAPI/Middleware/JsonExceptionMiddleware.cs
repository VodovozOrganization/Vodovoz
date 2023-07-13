using DriverAPI.DTOs.V2;
using DriverAPI.Library.Models;
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

		// TODO: Убрать эти костыли и переработать API
		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch(DataNotFoundException dataNotFoundException)
			{
				await MapException(context, dataNotFoundException, StatusCodes.Status400BadRequest);
			}
			catch(AccessViolationException accessViolationException)
			{
				await MapException(context, accessViolationException, StatusCodes.Status400BadRequest);
			}
			catch(UnauthorizedAccessException exception)
			{
				await MapException(context, exception, StatusCodes.Status401Unauthorized);
			}
			catch(ArgumentOutOfRangeException exception)
			{
				await MapException(context, exception, StatusCodes.Status400BadRequest);
			}
			catch(InvalidOperationException exception)
			{
				await MapException(context, exception, StatusCodes.Status400BadRequest);
			}
			catch(InvalidTimeZoneException exception)
			{
				await MapException(context, exception, StatusCodes.Status400BadRequest);
			}
			catch(Exception exception)
			{
				await MapException(context, exception, StatusCodes.Status500InternalServerError);
			}
		}

		private async Task MapException(HttpContext context, Exception exception, int code)
		{
			_logger.LogError(exception, exception.Message);

			context.Response.StatusCode = code;
			context.Response.ContentType = "application/json; charset=utf-8";

			await context.Response.Body.FlushAsync();

			await context.Response.Body
				.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer
					.Serialize(new ErrorResponseDto(exception.Message))));
		}
	}
}
