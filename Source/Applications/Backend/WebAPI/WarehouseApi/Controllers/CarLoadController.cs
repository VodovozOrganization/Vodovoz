﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Errors;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Requests;
using WarehouseApi.Contracts.Responses;
using WarehouseApi.Filters;
using WarehouseApi.Library.Common;
using WarehouseApi.Library.Services;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace WarehouseApi.Controllers
{
	[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[WarehouseErrorHandlingFilter]
	[OnlyOneSession]
	[Route("/api/[action]")]
	public class CarLoadController : ControllerBase
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);
		private const string _exceptionMessage =
			"Внутренняя ошибка сервера. Обратитесь в техподдержку";

		private readonly ILogger<CarLoadController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ICarLoadService _carLoadService;

		public CarLoadController(
			ILogger<CarLoadController> logger,
			UserManager<IdentityUser> userManager,
			ICarLoadService carLoadService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_carLoadService = carLoadService ?? throw new ArgumentNullException(nameof(carLoadService));
		}

		/// <summary>
		/// Начало погрузки по талону погрузки погрузки
		/// </summary>
		/// <param name="documentId"></param>
		/// <returns><see cref="StartLoadResponse"/></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StartLoadResponse))]
		public async Task<IActionResult> StartLoad([FromQuery] int documentId, CancellationToken cancellationToken)
		{
			AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var accessTokenValue);
			var accessToken = accessTokenValue?.Parameter ?? string.Empty;
			var user = await _userManager.GetUserAsync(User);

			_logger.LogInformation("Запрос начала погрузки талона погрузки авто. DocumentId: {DocumentId}. User token: {AccessToken}",
				documentId,
				accessToken);

			try
			{
				var requestProcessingResult = await _carLoadService.StartLoad(documentId, user.UserName, accessToken, cancellationToken);

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Получение информации о заказе
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns><see cref="GetOrderResponse"/></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrderResponse))]
		public async Task<IActionResult> GetOrder([FromQuery] int orderId)
		{
			_logger.LogInformation("Запрос получения информации о заказе. OrderId: {OrderId}. User token: {AccessToken}",
				orderId,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var requestProcessingResult = await _carLoadService.GetOrder(orderId);

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Добавление отсканированного кода маркировки ЧЗ в заказ
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns><see cref="AddOrderCodeResponse"/></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddOrderCodeResponse))]
		public async Task<IActionResult> AddOrderCode(AddOrderCodeRequest requestData, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Запрос добавления кода ЧЗ в заказ. OrderId: {OrderId}, NomenclatureId: {NomenclatureId}, Code: {Code}. User token: {AccessToken}",
				requestData.OrderId,
				requestData.NomenclatureId,
				requestData.Code,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);

			try
			{
				var requestProcessingResult =
					await _carLoadService.AddOrderCode(requestData.OrderId, requestData.NomenclatureId, requestData.Code, user.UserName, cancellationToken);

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Замена отсканированного кода ЧЗ номенклатуры в заказе
		/// </summary>
		/// <returns><see cref="ChangeOrderCodeResponse"/></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeOrderCodeResponse))]
		public async Task<IActionResult> ChangeOrderCode(ChangeOrderCodeRequest requestData, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Запрос замены кода ЧЗ в заказе." +
				" OrderId: {OrderId}, NomenclatureId: {NomenclatureId}, OldCode: {OldCode}, NewCode: {NewCode}. User token: {AccessToken}",
				requestData.OrderId,
				requestData.NomenclatureId,
				requestData.OldCode,
				requestData.NewCode,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);

			try
			{
				var requestProcessingResult =
					await _carLoadService.ChangeOrderCode(
						requestData.OrderId,
						requestData.NomenclatureId,
						requestData.OldCode,
						requestData.NewCode,
						user.UserName,
						cancellationToken);

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Завершение погрузки по талону погрузки
		/// </summary>
		/// <returns><see cref="EndLoadResponse"/></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EndLoadResponse))]
		public async Task<IActionResult> EndLoad([FromQuery] int documentId, CancellationToken cancellationToken)
		{
			AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var accessTokenValue);
			var accessToken = accessTokenValue?.Parameter ?? string.Empty;
			var user = await _userManager.GetUserAsync(User);

			_logger.LogInformation("Запрос завершения погрузки талона погрузки авто. DocumentId: {DocumentId}. User token: {AccessToken}",
				documentId,
				accessToken);

			try
			{
				var requestProcessingResult = await _carLoadService.EndLoad(documentId, user.UserName, accessToken, cancellationToken);

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return GetProblemResult();
			}
		}

		private IActionResult MapRequestProcessingResult<TValue>(
			RequestProcessingResult<TValue> processingResult,
			Func<Result, int?> statusCodeSelectorFunc)
		{
			if(processingResult.Result.IsSuccess)
			{
				HttpContext.Response.StatusCode = statusCodeSelectorFunc(processingResult.Result) ?? StatusCodes.Status200OK;
				return new ObjectResult(processingResult.Result.Value);
			}

			HttpContext.Response.StatusCode = statusCodeSelectorFunc(processingResult.Result) ?? StatusCodes.Status400BadRequest;
			return new ObjectResult(processingResult.FailureData);
		}

		private IActionResult GetProblemResult(string exceptionMessage = null)
		{
			var response = new WarehouseApiResponseBase
			{
				Result = OperationResultEnumDto.Error,
				Error = string.IsNullOrWhiteSpace(exceptionMessage) ? _exceptionMessage : exceptionMessage,
			};

			return new ObjectResult(response)
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};
		}

		private int GetStatusCode(Result result)
		{
			if(result.IsSuccess)
			{
				return StatusCodes.Status200OK;
			}

			var firstError = result.Errors.FirstOrDefault();

			if(firstError != null
				&& (firstError.Code == CarLoadDocumentErrors.DocumentNotFound
					|| firstError.Code == CarLoadDocumentErrors.OrderNotFound
					|| firstError.Code == CarLoadDocumentErrors.CarLoadDocumentItemNotFound
					|| firstError.Code == TrueMarkCodeErrors.TrueMarkCodeForCarLoadDocumentItemNotFound))
			{
				return StatusCodes.Status404NotFound;
			}

			return StatusCodes.Status400BadRequest;
		}
	}
}
