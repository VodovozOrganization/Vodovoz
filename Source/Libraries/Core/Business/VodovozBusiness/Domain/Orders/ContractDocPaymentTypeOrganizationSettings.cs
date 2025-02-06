using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Orders
{
	public class ContractDocPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.ContractDocumentation;
	}
}
