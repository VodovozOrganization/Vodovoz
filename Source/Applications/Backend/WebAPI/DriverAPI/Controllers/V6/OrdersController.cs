using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Services;
using Edo.Transport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class OrdersController : VersionedController
	{
		private readonly IEmployeeService _employeeService;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IOrderService _orderService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly MessageService _edoMessageService;
		private static readonly ConcurrentDictionary<string, bool> _completeOrderDeliveryInProgress = new ConcurrentDictionary<string, bool>();

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
			IActionTimeHelper actionTimeHelper,
			MessageService edoMessageService
			) : base(logger)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordService));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_edoMessageService = edoMessageService ?? throw new ArgumentNullException(nameof(edoMessageService));
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

					if(firstError == Vodovoz.Errors.Orders.OrderErrors.NotFound
						|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotFoundAssociatedWithOrder
						|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFoundAssociatedWithOrder)
					{
						return StatusCodes.Status404NotFound;
					}

					return StatusCodes.Status500InternalServerError;
				});
		}

		/// <summary>
		/// Завершение доставки заказа
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="completedOrderRequestModel"><see cref="CompletedOrderRequest"/></param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> CompleteOrderDeliveryAsync([FromServices] IUnitOfWork unitOfWork, [FromBody] CompletedOrderRequest completedOrderRequestModel)
		{
			_logger.LogInformation("(Завершение заказа: {OrderId}) пользователем {Username} | User token: {AccessToken} | Тело запроса: {@RequestBody}",
				completedOrderRequestModel.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization],
				completedOrderRequestModel);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			if(!_completeOrderDeliveryInProgress.TryAdd($"{user.UserName}:{completedOrderRequestModel.OrderId}", true))
			{
				_logger.LogWarning("Запрос на завершение заказа {OrderId} уже в процессе обработки", completedOrderRequestModel.OrderId);
				HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
				return NoContent();
			}

			Activity.Current?.AddTag("OrderId", completedOrderRequestModel.OrderId);

			var resultMessage = "OK";

			var localActionTime = completedOrderRequestModel.ActionTimeUtc.ToLocalTime();

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				_completeOrderDeliveryInProgress.TryRemove($"{user.UserName}:{completedOrderRequestModel.OrderId}", out var _);

				Activity.Current?.SetStatus(ActivityStatusCode.Error);
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				var transaction = unitOfWork.Session.BeginTransaction();

				var result = await _orderService.CompleteOrderDelivery(
					recievedTime,
					driver,
					completedOrderRequestModel,
					completedOrderRequestModel);

				unitOfWork.Commit();

				if(result is Result<int>)
				{
					var resultWithMessage = (Result<int>)result;
					await _edoMessageService.PublishEdoRequestCreatedEvent(resultWithMessage.Value);
				}

				return MapResult(
					result,
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotEnRouteState)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == Vodovoz.Errors.Orders.OrderErrors.NotFound
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotFoundAssociatedWithOrder
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFoundAssociatedWithOrder)
						{
							return StatusCodes.Status404NotFound;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(MySqlException mysqlException) when(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry
				|| (mysqlException.InnerException is MySqlException innerMysqlException && innerMysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry))
			{
				_logger.LogError(mysqlException, "Произошла ошибка при сохранении завершения доставки заказа {OrderId}: {ExceptionMessage}",
					completedOrderRequestModel.OrderId,
					mysqlException.Message);

				var currentTransaction = unitOfWork.Session?
					.GetCurrentTransaction();

				if(currentTransaction != null && currentTransaction.IsActive)
				{
					currentTransaction.Rollback();
					currentTransaction.Dispose();
				}

				Activity.Current?.SetStatus(ActivityStatusCode.Error);
				return Problem($"Произошла ошибка при завершении доставки заказа {completedOrderRequestModel.OrderId}", statusCode: StatusCodes.Status400BadRequest);
			}
			catch(Exception ex) when(ex.InnerException is MySqlException innerMysqlException && innerMysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
			{
				_logger.LogError(ex, "Произошла ошибка при сохранении завершения доставки заказа {OrderId}: {ExceptionMessage}",
					completedOrderRequestModel.OrderId,
					ex.Message);

				var currentTransaction = unitOfWork.Session?
					.GetCurrentTransaction();

				if(currentTransaction != null && currentTransaction.IsActive)
				{
					currentTransaction.Rollback();
					currentTransaction.Dispose();
				}

				Activity.Current?.SetStatus(ActivityStatusCode.Error);
				return Problem($"Произошла ошибка при завершении доставки заказа {completedOrderRequestModel.OrderId}", statusCode: StatusCodes.Status400BadRequest);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении доставки заказа {OrderId}: {ExceptionMessage}",
					completedOrderRequestModel.OrderId,
					ex.Message);

				resultMessage = ex.Message;

				var currentTransaction = unitOfWork.Session?
					.GetCurrentTransaction();

				if(currentTransaction != null && currentTransaction.IsActive)
				{
					currentTransaction.Rollback();
					currentTransaction.Dispose();
				}

				Activity.Current?.SetStatus(ActivityStatusCode.Error);
				return Problem($"Произошла ошибка при завершении доставки заказа {completedOrderRequestModel.OrderId}");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(driver, DriverMobileAppActionType.CompleteOrderClicked, localActionTime, recievedTime, resultMessage);
				_completeOrderDeliveryInProgress.TryRemove($"{user.UserName}:{completedOrderRequestModel.OrderId}", out var _);
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
				await _orderService.UpdateOrderShipmentInfoAsync(
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

					if(firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState
						|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotEnRouteState)
					{
						return StatusCodes.Status400BadRequest;
					}

					if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
					{
						return StatusCodes.Status403Forbidden;
					}

					if(firstError == Vodovoz.Errors.Orders.OrderErrors.NotFound
						|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotFoundAssociatedWithOrder
						|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFoundAssociatedWithOrder)
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

						if(firstError == Vodovoz.Errors.Orders.OrderErrors.NotInOnTheWayStatus)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.OrderAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == Vodovoz.Errors.Orders.OrderErrors.NotFound
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotFoundAssociatedWithOrder)
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
	}
}
