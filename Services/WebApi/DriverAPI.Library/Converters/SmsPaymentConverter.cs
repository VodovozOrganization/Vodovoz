using DriverAPI.Library.Models;
using Vodovoz.Domain;

namespace DriverAPI.Library.Converters
{
	public class SmsPaymentStatusConverter
	{
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
					throw new ConverterException(nameof(smsPaymentStatus), smsPaymentStatus, $"Значение {smsPaymentStatus} не поддерживается");
			}
		}
	}
}
