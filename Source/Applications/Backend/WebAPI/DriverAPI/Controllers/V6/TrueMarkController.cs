using DriverApi.Contracts.V6.Requests;
using DriverApi.Contracts.V6.Responses;
using DriverAPI.Library.V6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Errors;
using OrderErrors = Vodovoz.Errors.Orders.OrderErrors;
using RouteListErrors = Vodovoz.Errors.Logistics.RouteListErrors;
using RouteListItemErrors = Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер маркировки Честного Знака
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class TrueMarkController : VersionedController
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmployeeService _employeeService;
		private readonly IOrderService _orderService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;

		/// <summary>
		/// Уонструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="employeeService"></param>
		/// <param name="orderService"></param>
		/// <param name="driverMobileAppActionRecordService"></param>
		public TrueMarkController(
			ILogger<TrueMarkController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeService employeeService,
			IOrderService orderService,
			IDriverMobileAppActionRecordService driverMobileAppActionRecordService)
			: base(logger)
		{
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService
				?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordService));
		}

		/// <summary>
		/// Добавление кода ЧЗ для заказа (адреса в МЛ)
		/// </summary>
		/// <param name="addOrderCodeRequestModel"><see cref="AddOrderCodeRequest"/></param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrueMarkCodeProcessingResultResponse))]
		public async Task<IActionResult> AddOrderCode([FromBody] AddOrderCodeRequest addOrderCodeRequestModel, CancellationToken cancellationToken)
		{
			_logger.LogInformation("(Добавление кода ЧЗ к заказу: {OrderId}) пользователем {Username} | User token: {AccessToken} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				addOrderCodeRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			try
			{
				var requestProcessingResult =
					await _orderService.AddTrueMarkCode(
						recievedTime,
						driver,
						addOrderCodeRequestModel.OrderId,
						addOrderCodeRequestModel.OrderSaleItemId,
						addOrderCodeRequestModel.Code,
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
				_logger.LogError(ex, "При добавлении кода ЧЗ в строку заказ произошла ошибка. " +
					"OrderId: {OrderId}, " +
					"OrderItemId: {OrderSaleItemId}, " +
					"ExceptionMessage: {ExceptionMessage}",
					 addOrderCodeRequestModel.OrderId,
					 addOrderCodeRequestModel.OrderSaleItemId,
					 ex.Message);

				return Problem($"При добавлении кода ЧЗ в строку заказ произошла ошибка. " +
					$"OrderId: {addOrderCodeRequestModel.OrderId}, " +
					$"OrderItemId: {addOrderCodeRequestModel.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(driver, DriverMobileAppActionType.CompleteOrderClicked, DateTime.Now, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Замена кода ЧЗ для заказа (адреса в МЛ)
		/// </summary>
		/// <param name="changeOrderCodeRequest"><see cref="ChangeOrderCodeRequest"/></param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrueMarkCodeProcessingResultResponse))]
		public async Task<IActionResult> ChangeOrderCode([FromBody] ChangeOrderCodeRequest changeOrderCodeRequest, CancellationToken cancellationToken)
		{
			_logger.LogInformation("(Замена кода ЧЗ в заказе: {OrderId}) пользователем {Username} | User token: {AccessToken} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				changeOrderCodeRequest.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			try
			{
				var requestProcessingResult =
					await _orderService.ChangeTrueMarkCode(
						recievedTime,
						driver,
						changeOrderCodeRequest.OrderId,
						changeOrderCodeRequest.OrderSaleItemId,
						changeOrderCodeRequest.OldCode,
						changeOrderCodeRequest.NewCode,
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
				_logger.LogError(ex, "При замене кода ЧЗ в строке заказа произошла ошибка. " +
					"OrderId: {OrderId}, " +
					"OrderItemId: {OrderSaleItemId}, " +
					"ExceptionMessage: {ExceptionMessage}",
					changeOrderCodeRequest.OrderId,
					changeOrderCodeRequest.OrderSaleItemId,
					ex.Message);

				return Problem($"При замене кода ЧЗ в строке заказа произошла ошибка. " +
					$"OrderId: {changeOrderCodeRequest.OrderId}, " +
					$"OrderItemId: {changeOrderCodeRequest.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}");
			}
		}

		/// <summary>
		/// Удаление кода ЧЗ для заказа (адреса в МЛ)
		/// </summary>
		/// <param name="deleteOrderCodeRequest"><see cref="DeleteOrderCodeRequest"/></param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrueMarkCodeProcessingResultResponse))]
		public async Task<IActionResult> DeleteOrderCode([FromBody] DeleteOrderCodeRequest deleteOrderCodeRequest, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"(Удаление кода ЧЗ в заказе: {OrderId}) пользователем {Username} | User token: {AccessToken} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				deleteOrderCodeRequest.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			try
			{
				var requestProcessingResult =
					await _orderService.RemoveTrueMarkCode(
						driver,
						deleteOrderCodeRequest.OrderId,
						deleteOrderCodeRequest.OrderSaleItemId,
						deleteOrderCodeRequest.DeletedCode,
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
				_logger.LogError(ex, "При удалении кода ЧЗ в строке заказа произошла ошибка. " +
					"OrderId: {OrderId}, " +
					"OrderItemId: {OrderSaleItemId}, " +
					"ExceptionMessage: {ExceptionMessage}",
					deleteOrderCodeRequest.OrderId,
					deleteOrderCodeRequest.OrderSaleItemId,
					ex.Message);

				return Problem($"При удалении кода ЧЗ в строке заказа произошла ошибка. " +
					$"OrderId: {deleteOrderCodeRequest.OrderId}, " +
					$"OrderItemId: {deleteOrderCodeRequest.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}");
			}
		}

		/// <summary>
		/// Добавление кодов ЧЗ для заказа (адреса в МЛ). 
		/// Используется для добавления нескольких кодов для заказов для собственных нужд в тех случаях,
		/// когда водитель отсканировал коды, но не смог завершить заказ по месту.
		/// </summary>
		/// <param name="sendOrderCodesRequestModel"><see cref="SendOrderCodesRequest"/></param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> SendOrderCodes([FromBody] SendOrderCodesRequest sendOrderCodesRequestModel, CancellationToken cancellationToken)
		{
			_logger.LogInformation("(Добавление кодов ЧЗ к заказу: {OrderId}) пользователем {Username} | User token: {AccessToken} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				sendOrderCodesRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			try
			{
				var result =
					await _orderService.SendTrueMarkCodes(
						recievedTime,
						driver,
						sendOrderCodesRequestModel.OrderId,
						sendOrderCodesRequestModel.ScannedBottles,
						sendOrderCodesRequestModel.UnscannedBottlesReason,
						cancellationToken);

				return MapResult(
					result,
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError == RouteListErrors.NotEnRouteState
							|| firstError == RouteListItemErrors.NotEnRouteState)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == OrderErrors.NotFound
							|| firstError == RouteListItemErrors.NotFound
							|| firstError == RouteListErrors.NotFoundAssociatedWithOrder
							|| firstError == RouteListItemErrors.NotFoundAssociatedWithOrder)
						{
							return StatusCodes.Status404NotFound;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при добавлении кодов ЧЗ к заказу {OrderId}: {ExceptionMessage}",
					sendOrderCodesRequestModel.OrderId,
					ex.Message);

				return Problem($"Произошла ошибка при добавлении кодов ЧЗ к заказу {sendOrderCodesRequestModel.OrderId}");
			}
		}

		/// <summary>
		/// Проверка кода Честного Знака
		/// </summary>
		/// <param name="code">Код ЧЗ для проверки</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckCodeResultResponse))]
		public async Task<IActionResult> CheckCode([FromQuery] string code, CancellationToken cancellationToken)
		{
			_logger.LogInformation("(Проверка кода ЧЗ: {Code}) пользователем {Username} | User token: {AccessToken} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				code,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var receivedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			try
			{
				var requestProcessingResult =
					await _orderService.CheckCode(code, cancellationToken);

				if(requestProcessingResult.Result.IsSuccess)
				{
					var maxIndex = requestProcessingResult.Result.Value.Codes?.Count ?? 0;
					for(int i = 0; i < maxIndex; i++)
					{
						requestProcessingResult.Result.Value.Codes.ElementAt(i).SequenceNumber = i;
					}
				}

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При проверке кода ЧЗ произошла ошибка. " +
					"Code: {Code}, " +
					"ExceptionMessage: {ExceptionMessage}",
					code,
					ex.Message);

				return Problem($"При проверке кода ЧЗ произошла ошибка. " +
					$"Code: {code}, " +
					$"ExceptionMessage: {ex.Message}");
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

		private static int GetStatusCode(Result result)
		{
			if(result.IsSuccess)
			{
				return StatusCodes.Status200OK;
			}

			var firstError = result.Errors.FirstOrDefault();

			if(firstError != null
				&& (firstError.Code == OrderErrors.NotFound
					|| firstError.Code == RouteListErrors.NotFound
					|| firstError.Code == RouteListItemErrors.NotFound
					|| firstError.Code == TrueMarkCodeErrors.TrueMarkCodeForRouteListItemNotFound))
			{
				return StatusCodes.Status404NotFound;
			}

			if(firstError != null && firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
			{
				return StatusCodes.Status403Forbidden;
			}

			return StatusCodes.Status400BadRequest;
		}
	}
}
