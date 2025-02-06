using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class DriverAppQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.DriverApplicationQR;
	}
}
