using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class CashlessPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cashless;
	}
}
