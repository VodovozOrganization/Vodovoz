using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
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

		protected IActionResult MapResult(HttpContext httpContext, Result result, int? statusCode = null, int? errorStatusCode = null)
		{
			if(result.IsSuccess)
			{
				httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status204NoContent;
				return new NoContentResult();
			}

			return MapErrors(httpContext.Request.Path, result.Errors, errorStatusCode);
		}

		protected IActionResult MapResult<TValue>(HttpContext httpContext, Result<TValue> result, int? statusCode = null, int? errorStatusCode = null)
		{
			if(result.IsSuccess)
			{
				httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status200OK;
				return new ObjectResult(result.Value);
			}

			return MapErrors(httpContext.Request.Path, result.Errors, errorStatusCode);
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
				Detail = GetErrorDisplayName(errorsList.First()) ?? errorsList.First().Message
			});
		}

		private string GetErrorDisplayName([CallerMemberName] string name = "")
		{
			var errorTypeString = name.Replace(name.Substring(name.LastIndexOf(".")), "");

			var errorsType = GetTypeByFullyQualifiedName(errorTypeString);

			var memberInfo = errorsType.GetMember(name.Substring(name.LastIndexOf(".") + 1));

			var attribute = memberInfo.First().GetAttribute<DisplayAttribute>();

			return attribute?.Name;
		}

		private Type GetTypeByFullyQualifiedName(string typeName)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

			foreach(var assembly in assemblies)
			{
				var type = assembly.GetType(typeName);

				if(type != null)
				{
					return type;
				}
			}

			throw new ArgumentException(
				"Type " + typeName + " doesn't exist in the current app domain");
		}
	}
}
