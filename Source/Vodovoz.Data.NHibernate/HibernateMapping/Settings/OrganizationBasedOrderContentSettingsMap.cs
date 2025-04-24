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
			
			HasManyToMany(x => x.Organizations)
				.Table("organizations_based_order_content_settings_organizations")
				.ParentKeyColumn("organizations_based_order_content_settings_id")
				.ChildKeyColumn("organization_id")
				.Not.LazyLoad();
			
			HasManyToMany(x => x.Nomenclatures)
				.Table("organizations_based_order_content_settings_nomenclatures")
				.ParentKeyColumn("organizations_based_order_content_settings_id")
				.ChildKeyColumn("nomenclature_id")
				.Not.LazyLoad();
			
			HasManyToMany(x => x.ProductGroups)
				.Table("organizations_based_order_content_settings_product_groups")
				.ParentKeyColumn("organizations_based_order_content_settings_id")
				.ChildKeyColumn("product_group_id")
				.Not.LazyLoad();
		}
	}
}
