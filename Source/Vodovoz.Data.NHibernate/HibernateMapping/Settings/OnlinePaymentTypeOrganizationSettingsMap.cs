using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class OnlinePaymentTypeOrganizationSettingsMap : SubclassMap<OnlinePaymentTypeOrganizationSettings>
	{
		public OnlinePaymentTypeOrganizationSettingsMap()
		{
			DiscriminatorValue(nameof(PaymentType.PaidOnline));

			HasManyToMany(x => x.PaymentsFrom)
				.Table("payment_types_organizations_settings_payments_from")
				.ParentKeyColumn("payment_types_organizations_settings_id")
				.ChildKeyColumn("payment_from_id");
		}
	}
}
