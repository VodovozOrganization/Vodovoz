using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.WebApi.ErrorHandling
{
	public class ErrorHandlingMiddleware
	{
		private readonly ILogger<ErrorHandlingMiddleware> _logger;
		private readonly RequestDelegate _next;

		public ErrorHandlingMiddleware(
			ILogger<ErrorHandlingMiddleware> logger,
			RequestDelegate next)
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
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			_logger.LogCritical(
				exception,
				"Произошла критическая ошибка: {ExceptionMessage}",
				exception.Message);

			var result = JsonSerializer.Serialize(
				new ProblemDetails
				{
					Title = "При обработке вышего запроса произошла ошибка.",
					Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
					Instance = context.Request.Path,
					Status = StatusCodes.Status500InternalServerError,
					Detail = exception.Message
				});

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = "application/json; charset=utf-8";

			await context.Response.WriteAsync(result);
		}
	}
}
