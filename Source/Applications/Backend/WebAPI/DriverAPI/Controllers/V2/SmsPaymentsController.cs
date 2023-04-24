using DriverAPI.Library.Converters;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Net.Http.Headers;
using Vodovoz.Domain.Logistic.Drivers;
using System.Threading.Tasks;
using DriverAPI.DTOs.V2;

namespace DriverAPI.Controllers.V2
{
	[ApiVersion("2.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	[Authorize]
	public class SmsPaymentsController : ControllerBase
	{
		private readonly ILogger<SmsPaymentsController> _logger;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly ISmsPaymentModel _aPISmsPaymentData;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly UserManager<IdentityUser> _userManager;

		public SmsPaymentsController(ILogger<SmsPaymentsController> logger,
			IConfiguration configuration,
			IActionTimeHelper actionTimeHelper,
			ISmsPaymentModel aPISmsPaymentData,
			SmsPaymentStatusConverter smsPaymentConverter,
			IOrderModel aPIOrderData,
			IEmployeeModel employeeData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel,
			UserManager<IdentityUser> userManager)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_aPISmsPaymentData = aPISmsPaymentData ?? throw new ArgumentNullException(nameof(aPISmsPaymentData));
			_smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_driverMobileAppActionRecordModel = driverMobileAppActionRecordModel ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Эндпоинт получения статуса оплаты через СМС
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>OrderPaymentStatusResponseModel или null</returns>
		[HttpGet]
		[Route("GetOrderSmsPaymentStatus")]
		public OrderSmsPaymentStatusResponseDto GetOrderSmsPaymentStatus(int orderId)
		{
			_logger.LogInformation("Запрос состояния оплаты заказа {OrderId} пользователем {Username} User token: {AccessToken}",
				orderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var additionalInfo = _aPIOrderData.GetAdditionalInfo(orderId)
				?? throw new Exception($"Не удалось получить информацию о заказе {orderId}");

			var response = new OrderSmsPaymentStatusResponseDto()
			{
				AvailablePaymentTypes = additionalInfo.AvailablePaymentTypes,
				CanSendSms = additionalInfo.CanSendSms,
				SmsPaymentStatus = _smsPaymentConverter.convertToAPIPaymentStatus(
					_aPISmsPaymentData.GetOrderSmsPaymentStatus(orderId)
				)
			};

			return response;
		}

		/// <summary>
		/// Эндпоинт запроса СМС для оплаты заказа
		/// </summary>
		/// <param name="payBySmsRequestModel"></param>
		[HttpPost]
		[Route("PayBySms")]
		public async Task PayBySmsAsync(PayBySmsRequestDto payBySmsRequestModel)
		{
			return;
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос СМС для оплаты заказа {OrderId} на номер {PhoneNumber} пользователем {Username} User token: {AccessToken}",
				payBySmsRequestModel.OrderId,
				payBySmsRequestModel.PhoneNumber,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			var actionTime = _actionTimeHelper.GetActionTime(payBySmsRequestModel);

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, actionTime);

				_aPIOrderData.SendSmsPaymentRequest(payBySmsRequestModel.OrderId, payBySmsRequestModel.PhoneNumber, driver.Id);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver,
					DriverMobileAppActionType.PayBySmsClicked,
					actionTime,
					recievedTime,
					resultMessage);
			}
		}
	}
}
