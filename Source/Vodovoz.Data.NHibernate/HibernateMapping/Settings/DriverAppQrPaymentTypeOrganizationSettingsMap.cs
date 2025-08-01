using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class DriverAppQrPaymentTypeOrganizationSettingsMap : SubclassMap<DriverAppQrPaymentTypeOrganizationSettings>
	{
		public DriverAppQrPaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.DriverApplicationQR));
		}
	}
}
