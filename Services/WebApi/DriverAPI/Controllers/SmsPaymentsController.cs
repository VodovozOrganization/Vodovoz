using DriverAPI.DTOs;
using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SmsPaymentsController : ControllerBase
	{
		private readonly ILogger<SmsPaymentsController> _logger;
		private readonly ISmsPaymentModel _aPISmsPaymentData;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly int _timeout;
		private readonly int _futureTimeout;

		public SmsPaymentsController(ILogger<SmsPaymentsController> logger,
			IConfiguration configuration,
			ISmsPaymentModel aPISmsPaymentData,
			SmsPaymentStatusConverter smsPaymentConverter,
			IOrderModel aPIOrderData,
			IEmployeeModel employeeData,
			UserManager<IdentityUser> userManager)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_aPISmsPaymentData = aPISmsPaymentData ?? throw new ArgumentNullException(nameof(aPISmsPaymentData));
			_smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_timeout = configuration.GetValue<int>("PostActionTimeTimeOut");
			_futureTimeout = configuration.GetValue<int>("FutureAtionTimeTimeOut");
		}

		/// <summary>
		/// Эндпоинт получения статуса оплаты через СМС
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>OrderPaymentStatusResponseModel или null</returns>
		[HttpGet]
		[Route("/api/GetOrderSmsPaymentStatus")]
		public OrderPaymentStatusResponseDto GetOrderSmsPaymentStatus(int orderId)
		{
			var additionalInfo = _aPIOrderData.GetAdditionalInfo(orderId)
				?? throw new Exception($"Не удалось получить информацию о заказе {orderId}");

			var response = new OrderPaymentStatusResponseDto()
			{
				AvailablePaymentTypes = additionalInfo.AvailablePaymentTypes,
				CanSendSms = additionalInfo.CanSendSms,
				SmsPaymentStatus = _smsPaymentConverter.convertToAPIPaymentStatus(
					_aPISmsPaymentData.GetOrderPaymentStatus(orderId)
				)
			};

			return response;
		}

		/// <summary>
		/// Эндпоинт запроса СМС для оплаты заказа
		/// </summary>
		/// <param name="payBySmsRequestModel"></param>
		[HttpPost]
		[Route("/api/PayBySms")]
		public void PayBySms(PayBySmsRequestDto payBySmsRequestModel)
		{
			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_logger.LogInformation($"Запрос смены оплаты заказа: { payBySmsRequestModel.OrderId }" +
				$" на оплату по СМС с номером { payBySmsRequestModel.PhoneNumber } пользователем {HttpContext.User.Identity?.Name ?? "Unknown"} ({driver?.Id})");

			_aPIOrderData.SendSmsPaymentRequest(payBySmsRequestModel.OrderId, payBySmsRequestModel.PhoneNumber, driver.Id);
		}
	}
}
