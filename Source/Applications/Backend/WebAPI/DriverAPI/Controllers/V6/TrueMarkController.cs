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
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Errors;
using Vodovoz.Presentation.WebApi.Common;
using OrderErrors = Vodovoz.Errors.Orders.Order;
using RouteListErrors = Vodovoz.Errors.Logistics.RouteList;
using RouteListItemErrors = Vodovoz.Errors.Logistics.RouteList.RouteListItem;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

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
			ILogger<ApiControllerBase> logger,
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

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				var errorMessage =
					$"При добавлении кода ЧЗ в строку заказ произошла ошибка. " +
					$"OrderId: {addOrderCodeRequestModel.OrderId}, " +
					$"OrderItemId: {addOrderCodeRequestModel.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}";

				_logger.LogError(ex, errorMessage);

				return Problem(errorMessage);
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

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				var errorMessage =
					$"При замене кода ЧЗ в строке заказа произошла ошибка. " +
					$"OrderId: {changeOrderCodeRequest.OrderId}, " +
					$"OrderItemId: {changeOrderCodeRequest.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}";

				_logger.LogError(ex, errorMessage);

				return Problem(errorMessage);
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

				return MapRequestProcessingResult(
					requestProcessingResult,
					result => GetStatusCode(result));
			}
			catch(Exception ex)
			{
				var errorMessage =
					$"При замене кода ЧЗ в строке заказа произошла ошибка. " +
					$"OrderId: {deleteOrderCodeRequest.OrderId}, " +
					$"OrderItemId: {deleteOrderCodeRequest.OrderSaleItemId}, " +
					$"ExceptionMessage: {ex.Message}";

				_logger.LogError(ex, errorMessage);

				return Problem(errorMessage);
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

		private int GetStatusCode(Result result)
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
