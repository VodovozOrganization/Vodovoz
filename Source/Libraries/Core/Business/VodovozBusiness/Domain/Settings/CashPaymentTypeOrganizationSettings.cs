using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class CashPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cash;
	}
}
