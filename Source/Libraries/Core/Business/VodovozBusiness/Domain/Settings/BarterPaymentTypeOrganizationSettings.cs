using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class BarterPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Barter;
	}
}
