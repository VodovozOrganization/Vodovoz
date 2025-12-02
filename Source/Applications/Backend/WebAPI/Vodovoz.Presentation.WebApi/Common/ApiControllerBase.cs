using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Presentation.WebApi.ErrorHandling;

namespace Vodovoz.Presentation.WebApi.Common
{
	[ApiController]
	[ErrorHandlingFilter]
	public class ApiControllerBase : ControllerBase
	{
		protected readonly ILogger<ApiControllerBase> _logger;

		public ApiControllerBase(ILogger<ApiControllerBase> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Маппинг результата к ответу сервера
		/// </summary>
		/// <param name="result">Результат, который требуется привести к ответу сервера</param>
		/// <param name="statusCodeSelectorFunc">Селектор Http-кода ответа сервера</param>
		/// <returns></returns>
		protected IActionResult MapResult(Result result, Func<Result, int?> statusCodeSelectorFunc)
		{
			if(result.IsSuccess)
			{
				HttpContext.Response.StatusCode = statusCodeSelectorFunc(result) ?? StatusCodes.Status204NoContent;
				return new NoContentResult();
			}

			return MapErrors(result.Errors, statusCodeSelectorFunc(result));
		}

		/// <summary>
		/// Маппинг результата к ответу сервера
		/// </summary>
		/// <typeparam name="TValue">Тип успешного ответа (тип содержащий тело ответа)</typeparam>
		/// <param name="result">Результат, который требуется привести к ответу сервера</param>
		/// <param name="statusCodeSelectorFunc">Селектор Http-кода ответа сервера</param>
		/// <returns></returns>
		protected IActionResult MapResult<TValue>(Result<TValue> result, Func<Result, int?> statusCodeSelectorFunc)
		{
			if(result.IsSuccess)
			{
				HttpContext.Response.StatusCode = statusCodeSelectorFunc(result) ?? StatusCodes.Status200OK;
				return new ObjectResult(result.Value);
			}

			return MapErrors(result.Errors, statusCodeSelectorFunc(result));
		}

		/// <summary>
		/// Маппинг результата к ответу сервера
		/// </summary>
		/// <param name="result">Результат, который требуется привести к ответу сервера</param>
		/// <param name="statusCode">Http-код статуса успешного ответа</param>
		/// <param name="errorStatusCode">Http-код статуса ответа с ошибкой</param>
		/// <returns></returns>
		protected IActionResult MapResult(Result result, int? statusCode = null, int? errorStatusCode = null)
		{
			if(result.IsSuccess)
			{
				HttpContext.Response.StatusCode = statusCode ?? StatusCodes.Status204NoContent;
				return new NoContentResult();
			}

			return MapErrors(result.Errors, errorStatusCode);
		}

		/// <summary>
		/// Маппинг результата к ответу сервера
		/// </summary>
		/// <typeparam name="TValue">Тип успешного ответа (тип содержащий тело ответа)</typeparam>
		/// <param name="httpContext">Http-контекст запроса</param>
		/// <param name="result">Результат, который требуется привести к ответу сервера</param>
		/// <param name="statusCode">Http-код статуса успешного ответа</param>
		/// <param name="errorStatusCode">Http-код статуса ответа с ошибкой</param>
		/// <returns></returns>
		protected IActionResult MapResult<TValue>(Result<TValue> result, int? statusCode = null, int? errorStatusCode = null)
		{
			if(result.IsSuccess)
			{
				HttpContext.Response.StatusCode = statusCode ?? StatusCodes.Status200OK;
				return new ObjectResult(result.Value);
			}

			return MapErrors(result.Errors, errorStatusCode);
		}

		/// <summary>
		/// Маппинг ошибок к <see cref="ProblemDetails"/>
		/// </summary>
		/// <param name="instance">Инстанс в котором произошла ошибка (путь к эндпоинту)</param>
		/// <param name="errors">Перечисление ошибок</param>
		/// <param name="statusCode">Http-код ответа сервера</param>
		/// <returns></returns>
		private IActionResult MapErrors(
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
				Instance = HttpContext.Request.Path,
				Title = GetErrorDisplayName(errorsList.First()) ?? "Произошла ошибка",
				Status = statusCode ?? StatusCodes.Status500InternalServerError,
				Detail = errorsList.First().Message
			});
		}

		/// <summary>
		/// Получение параметра Name у <see cref="DisplayAttribute"/> указанной ошибки
		/// </summary>
		/// <param name="error">ошибка</param>
		/// <returns></returns>
		private string GetErrorDisplayName(Error error)
		{
			return error?.Type?.GetAttribute<DisplayAttribute>()?.Name;
		}
	}
}
