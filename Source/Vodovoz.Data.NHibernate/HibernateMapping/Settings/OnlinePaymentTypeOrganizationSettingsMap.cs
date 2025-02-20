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

			Map(x => x.CriterionForOrganization).Column("criterion_for_organization");

			References(x => x.PaymentFrom).Column("payment_from_id");
		}
	}
}
