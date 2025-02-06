using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class DriverAppQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.DriverApplicationQR;
	}
}
