using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class OrganizationByOrderAuthorSettingsMap : ClassMap<OrganizationByOrderAuthorSettings>
	{
		public OrganizationByOrderAuthorSettingsMap()
		{
			Table("organizations_by_order_author_settings");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			References(x => x.OrganizationBasedOrderContentSettings)
				.Column("organizations_based_order_content_settings_id");
			
			HasManyToMany(x => x.OrderAuthorsSubdivisions)
				.Table("organizations_by_order_author_settings_subdivisions")
				.ParentKeyColumn("organizations_by_order_author_settings_id")
				.ChildKeyColumn("subdivision_id")
				.LazyLoad();
		}
	}
}
