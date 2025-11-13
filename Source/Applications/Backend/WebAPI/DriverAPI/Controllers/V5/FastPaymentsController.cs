using DriverApi.Contracts.V5.Requests;
using DriverApi.Contracts.V5.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V5.Converters;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Errors;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер оплат СБП
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class FastPaymentsController : VersionedController
	{
		private readonly ILogger<FastPaymentsController> _logger;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly IFastPaymentService _fastPaymentService;
		private readonly QrPaymentConverter _qrPaymentConverter;
		private readonly IOrderService _orderService;
		private readonly IEmployeeService _employeeService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="actionTimeHelper"></param>
		/// <param name="fastPaymentService"></param>
		/// <param name="qrPaymentConverter"></param>
		/// <param name="orderService"></param>
		/// <param name="employeeService"></param>
		/// <param name="driverMobileAppActionRecordService"></param>
		/// <param name="userManager"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public FastPaymentsController(
			ILogger<FastPaymentsController> logger,
			IActionTimeHelper actionTimeHelper,
			IFastPaymentService fastPaymentService,
			QrPaymentConverter qrPaymentConverter,
			IOrderService orderService,
			IEmployeeService employeeService,
			IDriverMobileAppActionRecordService driverMobileAppActionRecordService,
			UserManager<IdentityUser> userManager) : base(logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_actionTimeHelper = actionTimeHelper
				?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_fastPaymentService = fastPaymentService
				?? throw new ArgumentNullException(nameof(fastPaymentService));
			_qrPaymentConverter = qrPaymentConverter
				?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService
				?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordService));
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Получение статуса оплаты заказа посредством QR-кода
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>OrderPaymentStatusResponseModel или null</returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderQrPaymentStatusResponse))]
		public IActionResult GetOrderQRPaymentStatus(int orderId)
		{
			var additionalInfo = _orderService.GetAdditionalInfo(orderId);

			if(additionalInfo.IsFailure)
			{
				_logger.LogWarning("Не удалось получить информацию о заказе {OrderId}", orderId);

				return MapResult(additionalInfo, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			return Ok(new OrderQrPaymentStatusResponse
			{
				AvailablePaymentTypes = additionalInfo.Value.AvailablePaymentTypes,
				CanReceiveQR = additionalInfo.Value.CanReceiveQRCode,
				QRPaymentStatus = _qrPaymentConverter.ConvertToAPIPaymentStatus(_fastPaymentService.GetOrderFastPaymentStatus(orderId))
			});
		}

		/// <summary>
		/// Получение QR-кода для оплаты заказа
		/// </summary>
		/// <param name="payByQRRequestDTO"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PayByQrResponse))]
		public async Task<IActionResult> PayByQRAsync(PayByQrRequest payByQRRequestDTO)
		{
			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			_logger.LogInformation("Запрос смены оплаты заказа: {OrderId}" +
				" на оплату по QR-коду пользователем {Username} ({DriverId})",
				payByQRRequestDTO.OrderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				driver?.Id);

			var resultMessage = "OK";
			var localActionTime = payByQRRequestDTO.ActionTimeUtc.ToLocalTime();

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				if(payByQRRequestDTO.BottlesByStockActualCount.HasValue)
				{
					var updateBottlesCountResult = _orderService.UpdateBottlesByStockActualCount(payByQRRequestDTO.OrderId, payByQRRequestDTO.BottlesByStockActualCount.Value);
					if(updateBottlesCountResult.IsFailure)
					{
						return MapResult(updateBottlesCountResult, errorStatusCode: StatusCodes.Status400BadRequest);
					}
				}

				return MapResult(
					await _orderService.SendQrPaymentRequestAsync(payByQRRequestDTO.OrderId, driver.Id),
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status200OK;
						}

						var firstError = result.Errors.First();

						if(firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotEnRouteState)
						{
							return StatusCodes.Status400BadRequest;
						}

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
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при запросе QR-кода {ExceptionMessage}", ex.Message);

				resultMessage = ex.Message;

				return Problem("Произошла ошибка при запросе QR-кода");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(
					driver,
					DriverMobileAppActionType.PayByQRClicked,
					localActionTime,
					recievedTime,
					resultMessage);
			}
		}
	}
}
