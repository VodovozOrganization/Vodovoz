using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Presentation.WebApi.ErrorHandling
{
	public class ErrorHandlingFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(ExceptionContext context)
		{
			var logger = context.HttpContext.RequestServices
				.GetService<ILogger<ErrorHandlingFilterAttribute>>();

			var exception = context.Exception;

			logger.LogCritical(exception,
				"Произошла критическая ошибка: {ExceptionMessage}",
				exception.Message);

			var problemDetails = new ProblemDetails
			{
				Instance = context.HttpContext.Request.Path,
				Status = StatusCodes.Status500InternalServerError,
				Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
				Title = "При обработке вашего запроса произошла ошибка.",
				Detail = exception.Message
			};

			context.Result = new ObjectResult(problemDetails);

			context.ExceptionHandled = true;
		}
	}
}
