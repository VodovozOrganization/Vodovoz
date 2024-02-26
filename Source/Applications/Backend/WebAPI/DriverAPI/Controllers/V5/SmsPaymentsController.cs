using DriverApi.Contracts.V5;
using DriverAPI.Library.V5.Converters;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Mime;
using Vodovoz.Core.Domain.Employees;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер оплаты по смс
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class SmsPaymentsController : VersionedController
	{
		private readonly ILogger<SmsPaymentsController> _logger;
		private readonly ISmsPaymentService _smsPaymentService;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly IOrderService _orderService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="configuration"></param>
		/// <param name="smsPaymentService"></param>
		/// <param name="smsPaymentConverter"></param>
		/// <param name="orderService"></param>
		/// <exception cref="ArgumentNullException"></exception>

		public SmsPaymentsController(ILogger<SmsPaymentsController> logger,
			IConfiguration configuration,
			ISmsPaymentService smsPaymentService,
			SmsPaymentStatusConverter smsPaymentConverter,
			IOrderService orderService) : base(logger)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_smsPaymentService = smsPaymentService
				?? throw new ArgumentNullException(nameof(smsPaymentService));
			_smsPaymentConverter = smsPaymentConverter
				?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
		}

		/// <summary>
		/// Эндпоинт получения статуса оплаты через СМС
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns><see cref="OrderSmsPaymentStatusResponseDto"/></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderSmsPaymentStatusResponseDto))]
		[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
		public IActionResult GetOrderSmsPaymentStatus(int orderId)
		{
			_logger.LogInformation("Запрос состояния оплаты заказа {OrderId} пользователем {Username} User token: {AccessToken}",
				orderId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var additionalInfo = _orderService.GetAdditionalInfo(orderId);

			if(additionalInfo is null)
			{
				_logger.LogWarning("Не удалось получить информацию о заказе {OrderId}", orderId);

				return Problem("Не удалось получить информацию о заказе", statusCode: StatusCodes.Status400BadRequest);
			}

			return Ok(new OrderSmsPaymentStatusResponseDto()
			{
				AvailablePaymentTypes = additionalInfo.AvailablePaymentTypes,
				CanSendSms = additionalInfo.CanSendSms,
				SmsPaymentStatus = _smsPaymentConverter.ConvertToAPIPaymentStatus(
					_smsPaymentService.GetOrderSmsPaymentStatus(orderId)
				)
			});
		}
	}
}
