using DriverAPI.Library.DTOs;
using Vodovoz.Domain;

namespace DriverAPI.Library.Converters
{
	public class SmsPaymentStatusConverter
	{
		public SmsPaymentDtoStatus? convertToAPIPaymentStatus(SmsPaymentStatus? smsPaymentStatus)
		{
			if (smsPaymentStatus == null)
			{
				return null;
			}

			switch (smsPaymentStatus)
			{
				case SmsPaymentStatus.WaitingForPayment:
					return SmsPaymentDtoStatus.WaitingForPayment;
				case SmsPaymentStatus.Paid:
					return SmsPaymentDtoStatus.Paid;
				case SmsPaymentStatus.Cancelled:
					return SmsPaymentDtoStatus.Cancelled;
				default:
					throw new ConverterException(nameof(smsPaymentStatus), smsPaymentStatus, $"Значение {smsPaymentStatus} не поддерживается");
			}
		}
	}
}
