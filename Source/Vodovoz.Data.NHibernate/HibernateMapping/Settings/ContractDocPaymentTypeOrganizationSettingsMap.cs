using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class ContractDocPaymentTypeOrganizationSettingsMap : SubclassMap<ContractDocPaymentTypeOrganizationSettings>
	{
		public ContractDocPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.ContractDocumentation));
		}
	}
}
