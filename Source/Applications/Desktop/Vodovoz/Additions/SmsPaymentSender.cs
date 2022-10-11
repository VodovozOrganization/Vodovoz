using SmsPaymentService;
using VodovozInfrastructure.Utils;

namespace Vodovoz.Additions
{
	public class SmsPaymentSender
	{
		public PaymentResult SendSmsPaymentToNumber(int orderId, string phoneNumber)
		{
			var service = SmsPaymentServiceSetting.GetSmsPaymentService();
			if(service == null)
			{
				return new PaymentResult { ErrorDescription = "Сервис отправки Sms не работает, обратитесь в РПО." };
			}

			string realPhoneNumber = "8" + PhoneUtils.RemoveNonDigit(phoneNumber);
			return service.SendPayment(orderId, realPhoneNumber);
		}
	}
}
