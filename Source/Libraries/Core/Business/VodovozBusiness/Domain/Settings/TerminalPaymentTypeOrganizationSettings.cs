using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class TerminalPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Terminal;
	}
}
