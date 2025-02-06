using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class BarterPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Barter;
	}
}
