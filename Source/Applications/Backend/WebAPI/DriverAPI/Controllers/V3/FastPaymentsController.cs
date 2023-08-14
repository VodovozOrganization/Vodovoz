using DriverAPI.DTOs.V3;
using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Controllers.V3
{
	/// <summary>
	/// Контроллер оплат СБП
	/// </summary>
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	[Authorize]
	public class FastPaymentsController : ControllerBase
	{
		private readonly ILogger<SmsPaymentsController> _logger;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly IFastPaymentModel _fastPaymentModel;
		private readonly QRPaymentConverter _qrPaymentConverter;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="actionTimeHelper"></param>
		/// <param name="fastPaymentModel"></param>
		/// <param name="qrPaymentConverter"></param>
		/// <param name="aPIOrderData"></param>
		/// <param name="employeeData"></param>
		/// <param name="driverMobileAppActionRecordModel"></param>
		/// <param name="userManager"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public FastPaymentsController(ILogger<SmsPaymentsController> logger,
			IActionTimeHelper actionTimeHelper,
			IFastPaymentModel fastPaymentModel,
			QRPaymentConverter qrPaymentConverter,
			IOrderModel aPIOrderData,
			IEmployeeModel employeeData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel,
			UserManager<IdentityUser> userManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_driverMobileAppActionRecordModel =
				driverMobileAppActionRecordModel ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Получение статуса оплаты заказа посредством QR-кода
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>OrderPaymentStatusResponseModel или null</returns>
		[HttpGet]
		[Produces("application/json")]
		[Route("GetOrderQRPaymentStatus")]
		public OrderQRPaymentStatusResponseDto GetOrderQRPaymentStatus(int orderId)
		{
			var additionalInfo = _aPIOrderData.GetAdditionalInfo(orderId)
				?? throw new Exception($"Не удалось получить информацию о заказе {orderId}");

			var response = new OrderQRPaymentStatusResponseDto
			{
				AvailablePaymentTypes = additionalInfo.AvailablePaymentTypes,
				CanReceiveQR = additionalInfo.CanReceiveQRCode,
				QRPaymentStatus = _qrPaymentConverter.ConvertToAPIPaymentStatus(_fastPaymentModel.GetOrderFastPaymentStatus(orderId))
			};

			return response;
		}

		/// <summary>
		/// Получение QR-кода для оплаты заказа
		/// </summary>
		/// <param name="payByQRRequestDTO"></param>
		[HttpPost]
		[Produces("application/json")]
		[Route("PayByQR")]
		public async Task<PayByQRResponseDTO> PayByQR(PayByQRRequestDTO payByQRRequestDTO)
		{
			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

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
					_aPIOrderData.UpdateBottlesByStockActualCount(payByQRRequestDTO.OrderId, payByQRRequestDTO.BottlesByStockActualCount.Value);
				}

				return await _aPIOrderData.SendQRPaymentRequestAsync(payByQRRequestDTO.OrderId, driver.Id);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(
					driver,
					DriverMobileAppActionType.PayByQRClicked,
					localActionTime,
					recievedTime,
					resultMessage);
			}
		}
	}
}
