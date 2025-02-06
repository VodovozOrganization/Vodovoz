using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	public class ContractDocPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.ContractDocumentation;
	}
}
