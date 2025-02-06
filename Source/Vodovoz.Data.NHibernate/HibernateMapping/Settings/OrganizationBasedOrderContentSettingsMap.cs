using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Settings
{
	public class OrganizationBasedOrderContentSettingsMap : ClassMap<OrganizationBasedOrderContentSettings>
	{
		public OrganizationBasedOrderContentSettingsMap()
		{
			Table("organizations_based_order_content_settings");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.OrderContentSet).Column("order_content_set");
			
			References(x => x.Organization).Column("organization_id");
			
			HasManyToMany(x => x.Nomenclatures)
				.Table("organizations_based_order_content_settings_products")
				.ParentKeyColumn("organizations_based_order_content_settings_id")
				.ChildKeyColumn("nomenclature_id")
				.LazyLoad();
			
			HasManyToMany(x => x.ProductGroups)
				.Table("organizations_based_order_content_settings_products")
				.ParentKeyColumn("organizations_based_order_content_settings_id")
				.ChildKeyColumn("product_group_id")
				.LazyLoad();
		}
	}
}
