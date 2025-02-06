using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class CashlessPaymentTypeOrganizationSettingsMap : SubclassMap<CashlessPaymentTypeOrganizationSettings>
	{
		public CashlessPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.Cashless));
		}
	}
}
