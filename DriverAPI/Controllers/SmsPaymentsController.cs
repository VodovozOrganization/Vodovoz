using DriverAPI.Library.Converters;
using DriverAPI.Library.DataAccess;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SmsPaymentsController : ControllerBase
    {
        private readonly ILogger<SmsPaymentsController> logger;
        private readonly IAPISmsPaymentData aPISmsPaymentData;
        private readonly SmsPaymentConverter smsPaymentConverter;
        private readonly IAPIOrderData aPIOrderData;

        public SmsPaymentsController(ILogger<SmsPaymentsController> logger,
            IAPISmsPaymentData aPISmsPaymentData,
            SmsPaymentConverter smsPaymentConverter,
            IAPIOrderData aPIOrderData)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.aPISmsPaymentData = aPISmsPaymentData ?? throw new ArgumentNullException(nameof(aPISmsPaymentData));
            this.smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
            this.aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
        }

        /// <summary>
        /// Эндпоинт получения статуса оплаты через СМС
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <returns>OrderPaymentStatusResponseModel или null</returns>
        [HttpGet]
        [Route("/api/GetOrderSmsPaymentStatus")]
        public OrderPaymentStatusResponseModel GetOrderSmsPaymentStatus([FromBody] int orderId)
        {
            var additionalInfo = aPIOrderData.GetAdditionalInfoOrNull(orderId);

            if (additionalInfo == null)
            {
                return null;
            }

            return new OrderPaymentStatusResponseModel()
            {
                AvailablePaymentEnumTypes = additionalInfo.AvailablePaymentEnumTypes,
                CanSendSms = additionalInfo.CanSendSms,
                SmsPaymentStatusEnum = smsPaymentConverter.convertToAPIPaymentStatus(
                    aPISmsPaymentData.GetOrderPaymentStatus(orderId)
                )
            };
        }

        /// <summary>
        /// Эндпоинт запроса СМС для оплаты заказа
        /// </summary>
        /// <param name="payBySmsRequestModel"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Route("/api/PayBySms")]
        public IActionResult PayBySms(PayBySmsRequestModel payBySmsRequestModel)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
