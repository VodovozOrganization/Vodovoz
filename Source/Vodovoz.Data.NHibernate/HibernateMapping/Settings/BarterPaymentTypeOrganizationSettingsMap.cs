using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class BarterPaymentTypeOrganizationSettingsMap : SubclassMap<BarterPaymentTypeOrganizationSettings>
	{
		public BarterPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.Barter));
		}
	}
}
