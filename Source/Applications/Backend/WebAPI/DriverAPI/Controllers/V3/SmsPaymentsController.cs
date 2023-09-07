using DriverAPI.Library.Converters;
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
using DriverAPI.DTOs.V3;
using DriverAPI.Library.Helpers;

namespace DriverAPI.Controllers.V3
{
	/// <summary>
	/// Контроллер оплаты по смс
	/// </summary>
	[ApiVersion("3.0")]
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

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="configuration"></param>
		/// <param name="actionTimeHelper"></param>
		/// <param name="aPISmsPaymentData"></param>
		/// <param name="smsPaymentConverter"></param>
		/// <param name="aPIOrderData"></param>
		/// <param name="employeeData"></param>
		/// <param name="driverMobileAppActionRecordModel"></param>
		/// <param name="userManager"></param>
		/// <exception cref="ArgumentNullException"></exception>

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
		/// <returns><see cref="OrderSmsPaymentStatusResponseDto"/></returns>
		[HttpGet]
		[Produces("application/json")]
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
				SmsPaymentStatus = _smsPaymentConverter.ConvertToAPIPaymentStatus(
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
		[Produces("application/json")]
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
			var localActionTimel = payBySmsRequestModel.ActionTimeUtc.ToLocalTime();

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTimel);

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
					localActionTimel,
					recievedTime,
					resultMessage);
			}
		}
	}
}
