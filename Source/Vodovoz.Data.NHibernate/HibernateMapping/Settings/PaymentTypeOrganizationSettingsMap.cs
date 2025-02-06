using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class PaymentTypeOrganizationSettingsMap : ClassMap<PaymentTypeOrganizationSettings>
	{
		public PaymentTypeOrganizationSettingsMap()
		{
			Table("payment_types_organizations_settings");
			DiscriminateSubClassesOnColumn("payment_type");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			//Map(x => x.PaymentType).Column("payment_type").ReadOnly();
			
			References(x => x.OrganizationForOrder).Column("organization_id");
		}
	}
}
