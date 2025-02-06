using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class CashPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cash;
	}
}
