using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Domain;

namespace DriverAPI.Library.Converters
{
    public class SmsPaymentConverter
    {
        private readonly ILogger<SmsPaymentConverter> logger;

        public SmsPaymentConverter(ILogger<SmsPaymentConverter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public APISmsPaymentStatus? convertToAPIPaymentStatus(SmsPaymentStatus? smsPaymentStatus)
        {
            if (smsPaymentStatus == null)
            {
                return null;
            }

            switch (smsPaymentStatus)
            {
                case SmsPaymentStatus.WaitingForPayment:
                    return APISmsPaymentStatus.WaitingForPayment;
                case SmsPaymentStatus.Paid:
                    return APISmsPaymentStatus.Paid;
                case SmsPaymentStatus.Cancelled:
                    return APISmsPaymentStatus.Cancelled;
                default:
                    logger.LogWarning($"Не поддерживается тип: {smsPaymentStatus}");
                    throw new ArgumentException($"Не поддерживается тип: {smsPaymentStatus}");
            }
        }
    }
}
