using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class TerminalPaymentTypeOrganizationSettingsMap : SubclassMap<TerminalPaymentTypeOrganizationSettings>
	{
		public TerminalPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.Terminal));
		}
	}
}
