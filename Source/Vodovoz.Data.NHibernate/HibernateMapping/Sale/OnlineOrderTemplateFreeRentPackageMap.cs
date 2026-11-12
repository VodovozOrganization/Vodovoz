using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class OnlineOrderTemplateFreeRentPackageMap : ClassMap<OnlineOrderTemplateFreeRentPackage>
	{
		public OnlineOrderTemplateFreeRentPackageMap()
		{
			Table("online_orders_templates_free_rent_packages");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Count).Column("count").Not.Nullable();
			Map(x => x.TemplateId).Column("template_id").Not.Nullable();
			Map(x => x.Price).Column("price").Not.Nullable();
			
			References(x => x.FreeRentPackage).Column("free_rent_package_id").Not.Nullable();
		}
	}
}
