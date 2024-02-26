using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Errors;
using Vodovoz.Presentation.WebApi.ErrorHandling;

namespace Vodovoz.Presentation.WebApi.Common
{
	[ApiController]
	[ErrorHandlingFilter]
	[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
	public class ApiControllerBase : ControllerBase
	{
		private readonly ILogger<ApiControllerBase> _logger;

		public ApiControllerBase(ILogger<ApiControllerBase> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		protected IActionResult MapResult(HttpContext httpContext, Result result, int? statusCode = null)
		{
			if(result.IsSuccess)
			{
				httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status204NoContent;
				return new NoContentResult();
			}

			return MapErrors(httpContext.Request.Path, result.Errors, statusCode);
		}

		protected IActionResult MapResult<TValue>(HttpContext httpContext, Result<TValue> result, int? statusCode = null)
		{
			if(result.IsSuccess)
			{
				httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status200OK;
				return new ObjectResult(result.Value);
			}

			return MapErrors(httpContext.Request.Path, result.Errors, statusCode);
		}

		private IActionResult MapErrors(
			string instance,
			IEnumerable<Error> errors,
			int? statusCode = null)
		{
			var errorsList = errors.ToList();

			foreach(var error in errorsList)
			{
				_logger.LogWarning("Произошла ошибка: {Code} - {Message}", error.Code, error.Message);
			}

			return new ObjectResult(new ProblemDetails
			{
				Instance = instance,
				Title = "Произошла ошибка",
				Status = statusCode ?? StatusCodes.Status500InternalServerError,
				Detail = errorsList.First().Message
			});
		}
	}
}
