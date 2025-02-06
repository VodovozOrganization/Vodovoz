using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class SmsQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.SmsQR;
	}
}
