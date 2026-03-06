using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderTemplateProductMap : ClassMap<OnlineOrderTemplateProduct>
	{
		public OnlineOrderTemplateProductMap()
		{
			Table("online_orders_templates_products");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			Map(x => x.IsFixedPrice).Column("is_fixed_price");
			Map(x => x.OnlineOrderTemplateId).Column("template_id");
			
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.PromoSet).Column("promo_set_id");
			
			HasMany(x => x.Discounts)
				.KeyColumn("template_id")
				.Cascade
				.AllDeleteOrphan();
		}
	}
}
