using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class CashlessPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cashless;
	}
}
