using DriverAPI.DTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ErrorController : ControllerBase
	{
		private readonly ILogger<ErrorController> _logger;

		public ErrorController(ILogger<ErrorController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpDelete]
		[HttpGet]
		[HttpHead]
		[HttpOptions]
		[HttpPatch]
		[HttpPost]
		[HttpPut]
		[Route("/api/error")]
		public ErrorResponseDto Error()
		{
			var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
			var exception = context?.Error;
			var code = StatusCodes.Status500InternalServerError;

			if (exception != null)
			{
				_logger.LogError(exception, exception.Message);
			}
			else
			{
				exception = new Exception("Вызван обработчик ошибок без ошибки");
			}

			if(exception is UnauthorizedAccessException)
			{
				code = StatusCodes.Status401Unauthorized;
			}
			
			Response.StatusCode = code;

			return new ErrorResponseDto(exception.Message);
		}
	}
}
