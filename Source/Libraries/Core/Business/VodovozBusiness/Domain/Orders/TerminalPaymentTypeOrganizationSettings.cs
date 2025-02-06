using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class TerminalPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Terminal;
	}
}
