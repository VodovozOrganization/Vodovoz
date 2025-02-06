using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class SmsQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.SmsQR;
	}
}
