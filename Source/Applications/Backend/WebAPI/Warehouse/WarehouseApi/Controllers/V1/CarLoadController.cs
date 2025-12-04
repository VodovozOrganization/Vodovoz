using Microsoft.AspNetCore.Authorization;
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
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
<<<<<<<< HEAD:Source/Applications/Backend/WebAPI/WarehouseApi/Controllers/V1/CarLoadController.cs
using WarehouseApi.Contracts.Dto.V1;
using WarehouseApi.Contracts.Requests.V1;
using WarehouseApi.Contracts.Responses.V1;
========
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Contracts.V1.Requests;
using WarehouseApi.Contracts.V1.Responses;
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi/Controllers/V1/CarLoadController.cs
using WarehouseApi.Filters;
using WarehouseApi.Library.Services;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocumentErrors;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
<<<<<<<< HEAD:Source/Applications/Backend/WebAPI/WarehouseApi/Controllers/V1/CarLoadController.cs
	/// Контроллер для работы с талонами погрузки
========
	/// Контроллер талонов погрузки автомобилей
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi/Controllers/V1/CarLoadController.cs
	/// </summary>
	[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[WarehouseErrorHandlingFilter]
	[OnlyOneSession]
<<<<<<<< HEAD:Source/Applications/Backend/WebAPI/WarehouseApi/Controllers/V1/CarLoadController.cs
========
	[Route("/api/[action]")]
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi/Controllers/V1/CarLoadController.cs
	public class CarLoadController : VersionedController
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);
		private const string _exceptionMessage =
			"Внутренняя ошибка сервера. Обратитесь в техподдержку";

		private readonly UserManager<IdentityUser> _userManager;
		private readonly ICarLoadService _carLoadService;

		/// <summary>
<<<<<<<< HEAD:Source/Applications/Backend/WebAPI/WarehouseApi/Controllers/V1/CarLoadController.cs
		/// Конструктор контроллера
========
		/// Конструктор
>>>>>>>> origin/master:Source/Applications/Backend/WebAPI/Warehouse/WarehouseApi/Controllers/V1/CarLoadController.cs
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="carLoadService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public CarLoadController(
			ILogger<CarLoadController> logger,
			UserManager<IdentityUser> userManager,
			ICarLoadService carLoadService)
			: base(logger)
		{
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_carLoadService = carLoadService ?? throw new ArgumentNullException(nameof(carLoadService));
		}

		/// <summary>
		/// Начало погрузки по талону погрузки погрузки
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
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
				_logger.LogError(ex, "Произошла ошибка на стороне сервера: {ExceptionMessage}", ex.Message);
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

				if(requestProcessingResult.Result.IsSuccess)
				{
					foreach(var item in requestProcessingResult.Result.Value.Order.Items)
					{
						var maxIndex = item.Codes.Count;
						for(int i = 0; i < maxIndex; i++)
						{
							item.Codes[i].SequenceNumber = i;
						}
					}
				}

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка на стороне сервера: {ExceptionMessage}", ex.Message);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Добавление отсканированного кода маркировки ЧЗ в заказ
		/// </summary>
		/// <param name="requestData"></param>
		/// <param name="cancellationToken"></param>
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

				if(requestProcessingResult.Result.IsSuccess)
				{
					var maxIndex = requestProcessingResult.Result.Value.Nomenclature?.Codes.Count() ?? 0;
					for(int i = 0; i < maxIndex; i++)
					{
						requestProcessingResult.Result.Value.Nomenclature.Codes.ElementAt(i).SequenceNumber = i;
					}
				}

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка на стороне сервера: {ExceptionMessage}", ex.Message);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Замена отсканированного кода ЧЗ номенклатуры в заказе
		/// </summary>
		/// <param name="requestData"></param>
		/// <param name="cancellationToken"></param>
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

				if(requestProcessingResult.Result.IsSuccess)
				{
					var maxIndex = requestProcessingResult.Result.Value.Nomenclature?.Codes.Count() ?? 0;
					for(int i = 0; i < maxIndex; i++)
					{
						requestProcessingResult.Result.Value.Nomenclature.Codes.ElementAt(i).SequenceNumber = i;
					}
				}

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка на стороне сервера: {ExceptionMessage}", ex.Message);
				return GetProblemResult();
			}
		}

		/// <summary>
		/// Завершение погрузки по талону погрузки
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="cancellationToken"></param>
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
				_logger.LogError(ex, "Произошла ошибка на стороне сервера: {ExceptionMessage}", ex.Message);
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

			if(processingResult.FailureData == null)
			{
				var errorResult = GetProblemResult(string.Join(", ", processingResult.Result.Errors.Select(x => x.Message)));
				HttpContext.Response.StatusCode = statusCodeSelectorFunc(processingResult.Result) ?? StatusCodes.Status400BadRequest;
				return new ObjectResult(errorResult);
			}

			HttpContext.Response.StatusCode = statusCodeSelectorFunc(processingResult.Result) ?? StatusCodes.Status400BadRequest;
			return new ObjectResult(processingResult.FailureData);
		}

		private static IActionResult GetProblemResult(string exceptionMessage = null)
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

		private static int GetStatusCode(Result result)
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
