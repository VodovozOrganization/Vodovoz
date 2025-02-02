﻿using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using DriverApi.Contracts.V6.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Errors;
using OrderErrors = Vodovoz.Errors.Orders.Order;
using OrderItemErrors = Vodovoz.Errors.Orders.OrderItem;
using RouteListErrors = Vodovoz.Errors.Logistics.RouteList;
using RouteListItemErrors = Vodovoz.Errors.Logistics.RouteList.RouteListItem;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class OrdersController : VersionedController
	{
		private readonly ILogger<OrdersController> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IOrderService _orderService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;
		private readonly IActionTimeHelper _actionTimeHelper;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="employeeService"></param>
		/// <param name="userManager"></param>
		/// <param name="orderService"></param>
		/// <param name="driverMobileAppActionRecordService"></param>
		/// <param name="actionTimeHelper"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrdersController(
			ILogger<OrdersController> logger,
			IEmployeeService employeeService,
			UserManager<IdentityUser> userManager,
			IOrderService orderService,
			IDriverMobileAppActionRecordService driverMobileAppActionRecordService,
			IActionTimeHelper actionTimeHelper) : base(logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordService));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
		}

		/// <summary>
		/// Получение информации о заказе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
		public IActionResult GetOrder(int orderId)
		{
			_logger.LogInformation("(OrderId: {OrderId}) User token: {AccessToken}",
				orderId,
				Request.Headers[HeaderNames.Authorization]);

			return MapResult(
				_orderService.GetOrder(orderId),
				result =>
				{
					if(result.IsSuccess)
					{
						return StatusCodes.Status200OK;
					}

					var firstError = result.Errors.First();

					if(firstError == Library.Errors.Security.Authorization.RouteListAccessDenied)
					{
						return StatusCodes.Status403Forbidden;
					}

					if(firstError == Vodovoz.Errors.Orders.Order.NotFound
						|| firstError == Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder
						|| firstError == Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder)
					{
						return StatusCodes.Status404NotFound;
					}

					return StatusCodes.Status500InternalServerError;
				});
		}

		/// <summary>
		/// Завершение доставки заказа
		/// </summary>
		/// <param name="completedOrderRequestModel"><see cref="CompletedOrderRequest"/></param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> CompleteOrderDeliveryAsync([FromBody] CompletedOrderRequest completedOrderRequestModel)
		{
			_logger.LogInformation("(Завершение заказа: {OrderId}) пользователем {Username} | User token: {AccessToken}",
				completedOrderRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			var localActionTime = completedOrderRequestModel.ActionTimeUtc.ToLocalTime();

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				return MapResult(
					_orderService.CompleteOrderDelivery(
						recievedTime,
						driver,
						completedOrderRequestModel,
						completedOrderRequestModel),
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError == Vodovoz.Errors.Logistics.RouteList.NotEnRouteState
							|| firstError == Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == Vodovoz.Errors.Orders.Order.NotFound
							|| firstError == Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder
							|| firstError == Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder)
						{
							return StatusCodes.Status404NotFound;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении доставки заказа {OrderId}: {ExceptionMessage}",
					completedOrderRequestModel.OrderId,
					ex.Message);

				resultMessage = ex.Message;

				return Problem($"Произошла ошибка при завершении доставки заказа {completedOrderRequestModel.OrderId}");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(driver, DriverMobileAppActionType.CompleteOrderClicked, localActionTime, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Создание рекламации по координатам точки доставки заказа
		/// </summary>
		/// <param name="completedOrderRequestModel"></param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateOrderShipmentInfoAsync([FromBody] UpdateOrderShipmentInfoRequest completedOrderRequestModel)
		{
			_logger.LogInformation("(Создание рекламации по координатам точки доставки заказа: {OrderId}) пользователем {Username} | User token: {AccessToken}",
				completedOrderRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var localActionTime = completedOrderRequestModel.ActionTimeUtc.ToLocalTime();

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			return MapResult(
				_orderService.UpdateOrderShipmentInfo(
				recievedTime,
				driver,
				completedOrderRequestModel),
				result =>
				{
					if(result.IsSuccess)
					{
						return StatusCodes.Status204NoContent;
					}

					var firstError = result.Errors.First();

					if(firstError == Vodovoz.Errors.Logistics.RouteList.NotEnRouteState
						|| firstError == Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState)
					{
						return StatusCodes.Status400BadRequest;
					}

					if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
					{
						return StatusCodes.Status403Forbidden;
					}

					if(firstError == Vodovoz.Errors.Orders.Order.NotFound
						|| firstError == Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder
						|| firstError == Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder)
					{
						return StatusCodes.Status404NotFound;
					}

					return StatusCodes.Status500InternalServerError;
				});
		}

		/// <summary>
		/// Смены типа оплаты заказа
		/// </summary>
		/// <param name="changeOrderPaymentTypeRequestModel">Модель данных входящего запроса</param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> ChangeOrderPaymentTypeAsync(ChangeOrderPaymentTypeRequest changeOrderPaymentTypeRequestModel)
		{
			var recievedTime = DateTime.Now;

			var orderId = changeOrderPaymentTypeRequestModel.OrderId;
			var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;

			var localActionTime = changeOrderPaymentTypeRequestModel.ActionTimeUtc.ToLocalTime();

			_logger.LogInformation("Смена типа оплаты заказа: {OrderId} на {PaymentType}" +
				" на стороне мобильного приложения в {ActionTime} пользователем {Username} в {RecievedTime} | User token: {AccessToken}",
				orderId,
				newPaymentType,
				localActionTime,
				HttpContext.User.Identity?.Name ?? "Unknown",
				recievedTime,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				var availableToChangePaymentTypesResult = _orderService.GetAvailableToChangePaymentTypes(orderId);

				if(availableToChangePaymentTypesResult.IsFailure)
				{
					return MapResult(availableToChangePaymentTypesResult, StatusCodes.Status400BadRequest);
				}

				IEnumerable<PaymentDtoType> availableTypesToChange = availableToChangePaymentTypesResult.Value;

				if(!availableTypesToChange.Contains(newPaymentType))
				{
					_logger.LogWarning("Попытка сменить тип оплаты у заказа {OrderId} на недоступный для этого заказа тип оплаты {PaymentType}", orderId, newPaymentType);

					return Problem($"Попытка сменить тип оплаты у заказа {orderId} на недоступный для этого заказа тип оплаты {newPaymentType}", statusCode: StatusCodes.Status400BadRequest);
				}

				Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;
				Vodovoz.Domain.Client.PaymentByTerminalSource? paymentByTerminalSource = null; ;

				if(newPaymentType == PaymentDtoType.TerminalCard)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
					paymentByTerminalSource = Vodovoz.Domain.Client.PaymentByTerminalSource.ByCard;
				}
				else if(newPaymentType == PaymentDtoType.TerminalQR)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
					paymentByTerminalSource = Vodovoz.Domain.Client.PaymentByTerminalSource.ByQR;
				}
				else if(newPaymentType == PaymentDtoType.Cash)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Cash;
				}
				else if(newPaymentType == PaymentDtoType.DriverApplicationQR)
				{
					newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.DriverApplicationQR;
				}
				else
				{
					_logger.LogWarning("Попытка сменить тип оплаты у заказа {OrderId} на не поддерживаемый для смены тип оплаты {PaymentType}", orderId, newPaymentType);

					return Problem($"Попытка сменить тип оплаты у заказа {orderId} на не поддерживаемый для смены тип оплаты {newPaymentType}", statusCode: StatusCodes.Status400BadRequest);
				}

				return MapResult(
					_orderService.ChangeOrderPaymentType(orderId, newVodovozPaymentType, driver, paymentByTerminalSource),
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError == Vodovoz.Errors.Orders.Order.NotInOnTheWayStatus)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == Vodovoz.Errors.Orders.Order.NotFound
							|| firstError == Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder)
						{
							return StatusCodes.Status404NotFound;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при смене типа оплаты заказа {OrderId}: {ExceptionMessage}",
					changeOrderPaymentTypeRequestModel.OrderId,
					ex.Message);

				resultMessage = ex.Message;

				return Problem($"Произошла ошибка при смене типа оплаты заказа доставки заказа {changeOrderPaymentTypeRequestModel.OrderId}");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(
					driver,
					DriverMobileAppActionType.ChangeOrderPaymentTypeClicked,
					localActionTime,
					recievedTime,
					resultMessage);
			}
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
			_logger.LogInformation("(Добавление кода ЧЗ к заказу: {OrderId}) пользователем {Username} | User token: {AccessToken}",
				addOrderCodeRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

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
