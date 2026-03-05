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
			
			//Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			Map(x => x.Discount).Column("discount");
			Map(x => x.IsDiscountInMoney).Column("is_discount_in_money");
			Map(x => x.IsFixedPrice).Column("is_fixed_price");
			//Map(x => x.PromoSetId).Column("promo_set_id");
			Map(x => x.OnlineOrderTemplateId).Column("template_id");
			
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.PromoSet).Column("promo_set_id");
			References(x => x.DiscountReason).Column("discount_reason_id");
		}
	}
}
