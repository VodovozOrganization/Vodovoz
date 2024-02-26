using DriverApi.Contracts.V5;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V5.Converters;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

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
		private readonly QRPaymentConverter _qrPaymentConverter;
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
			QRPaymentConverter qrPaymentConverter,
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
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderQRPaymentStatusResponseDto))]
		[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
		public IActionResult GetOrderQRPaymentStatus(int orderId)
		{
			var additionalInfo = _orderService.GetAdditionalInfo(orderId);

			if(additionalInfo is null)
			{
				_logger.LogWarning("Не удалось получить информацию о заказе {OrderId}", orderId);

				return Problem($"Не удалось получить информацию о заказе {orderId}", statusCode: StatusCodes.Status400BadRequest);
			}

			return Ok(new OrderQRPaymentStatusResponseDto
			{
				AvailablePaymentTypes = additionalInfo.AvailablePaymentTypes,
				CanReceiveQR = additionalInfo.CanReceiveQRCode,
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
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PayByQRResponseDTO))]
		[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
		public async Task<IActionResult> PayByQRAsync(PayByQRRequestDTO payByQRRequestDTO)
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

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTime);

				if(payByQRRequestDTO.BottlesByStockActualCount.HasValue)
				{
					_orderService.UpdateBottlesByStockActualCount(payByQRRequestDTO.OrderId, payByQRRequestDTO.BottlesByStockActualCount.Value);
				}

				return Ok(await _orderService.SendQRPaymentRequestAsync(payByQRRequestDTO.OrderId, driver.Id));
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
