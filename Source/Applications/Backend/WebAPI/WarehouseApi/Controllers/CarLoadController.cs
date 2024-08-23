using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using WarehouseApi.Contracts.Requests;
using WarehouseApi.Contracts.Responses;
using WarehouseApi.Library.Services;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;

namespace WarehouseApi.Controllers
{
	[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[OnlyOneSession]
	[Route("/api/")]
	public class CarLoadController : ApiControllerBase
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);

		private readonly ILogger<CarLoadController> _logger;
		private readonly ICarLoadService _carLoadService;

		public CarLoadController(
			ILogger<CarLoadController> logger,
			ICarLoadService carLoadService) : base(logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_carLoadService = carLoadService ?? throw new System.ArgumentNullException(nameof(carLoadService));
		}

		/// <summary>
		/// Начало погрузки по талону погрузки погрузки
		/// </summary>
		/// <param name="documentId"></param>
		/// <returns></returns>
		[HttpPost("StartLoad")]
		public async Task<IActionResult> StartLoad([FromQuery] int documentId)
		{
			_logger.LogInformation("Запрос начала погрузки талона погрузки авто. DocumentId: {DocumentId}. User token: {AccessToken}",
				documentId,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var result = await _carLoadService.StartLoad(documentId);

				if(result.IsSuccess)
				{
					return MapResult(result);
				}

				return MapFailureValueResult(
					result,
					result =>
					{
						var firstError = result.Errors.FirstOrDefault();

						if(firstError != null && firstError.Code == CarLoadDocumentErrors.DocumentNotFound)
						{
							return HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
						}

						return HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);

				return Problem("Внутренняя ошибка сервера. Обратитесь в техподдержку", statusCode: 500);
			}
		}

		/// <summary>
		/// Получение информации о заказе
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("GetOrder")]
		public async Task<IActionResult> GetOrder([FromQuery] int orderId)
		{
			_logger.LogInformation("Запрос получения информации о заказе. OrderId: {OrderId}. User token: {AccessToken}",
				orderId,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var result = await _carLoadService.GetOrder(orderId);

				if(result.IsSuccess)
				{
					return MapResult(result);
				}

				return MapFailureValueResult(
					result,
					result =>
					{
						var firstError = result.Errors.FirstOrDefault();

						if(firstError != null && firstError.Code == CarLoadDocumentErrors.OrderNotFound)
						{
							return HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
						}

						return HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);

				return Problem("Внутренняя ошибка сервера. Обратитесь в техподдержку", statusCode: 500);
			}
		}

		/// <summary>
		/// Добавление отсканированного кода маркировки ЧЗ в заказ
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		[HttpPost("AddOrderCode")]
		public async Task<IActionResult> AddOrderCode(AddOrderCodeRequest requestData)
		{
			_logger.LogInformation("Запрос добавления кода ЧЗ в заказ. OrderId: {OrderId}, NomenclatureId: {NomenclatureId}, Code: {Code}. User token: {AccessToken}",
				requestData.OrderId,
				requestData.NomenclatureId,
				requestData.Code,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var result = await _carLoadService.AddOrderCode(requestData.OrderId, requestData.NomenclatureId, requestData.Code);

				if(result.IsSuccess)
				{
					return MapResult(result);
				}

				return MapFailureValueResult(
					result,
					result =>
					{
						return HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);

				return Problem("Внутренняя ошибка сервера. Обратитесь в техподдержку", statusCode: 500);
			}
		}

		/// <summary>
		/// Замена отсканированного кода ЧЗ номенклатуры в заказе
		/// </summary>
		/// <returns></returns>
		[HttpPost("ChangeOrderCode")]
		public async Task<IActionResult> ChangeOrderCode(ChangeOrderCodeRequest requestData)
		{
			_logger.LogInformation("Запрос замены кода ЧЗ в заказе." +
				" OrderId: {OrderId}, NomenclatureId: {NomenclatureId}, OldCode: {OldCode}, NewCode: {NewCode}. User token: {AccessToken}",
				requestData.OrderId,
				requestData.NomenclatureId,
				requestData.OldCode,
				requestData.NewCode,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var result =
					await _carLoadService.ChangeOrderCode(requestData.OrderId, requestData.NomenclatureId, requestData.OldCode, requestData.NewCode);

				if(result.IsSuccess)
				{
					return MapResult(result);
				}

				return MapFailureValueResult(
					result,
					result =>
					{
						return HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);

				return Problem("Внутренняя ошибка сервера. Обратитесь в техподдержку", statusCode: 500);
			}
		}

		/// <summary>
		/// Завершение погрузки по талону погрузки
		/// </summary>
		/// <returns></returns>
		[HttpPost("EndLoad")]
		public async Task<IActionResult> EndLoad([FromQuery] int documentId)
		{
			_logger.LogInformation("Запрос завершения погрузки талона погрузки авто. DocumentId: {DocumentId}. User token: {AccessToken}",
				documentId,
				Request.Headers[HeaderNames.Authorization]);

			try
			{
				var result = await _carLoadService.EndLoad(documentId);
				throw new Exception();
				if(result.IsSuccess)
				{
					return MapResult(result);
				}

				return MapFailureValueResult(
					result,
					result =>
					{
						var firstError = result.Errors.FirstOrDefault();

						if(firstError != null && firstError.Code == CarLoadDocumentErrors.DocumentNotFound)
						{
							return HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
						}

						return HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);

				//return Problem("Внутренняя ошибка сервера. Обратитесь в техподдержку", statusCode: 500);
				return GetProblemResult("Внутренняя ошибка сервера. Обратитесь в техподдержку");
			}
		}

		private IActionResult GetProblemResult(string message)
		{
			var response = new WarehouseApiResponseBase
			{
				Result = Contracts.Dto.OperationResultEnumDto.Error,
				Error = message,
			};

			return new ObjectResult(response)
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};
		}
	}
}
