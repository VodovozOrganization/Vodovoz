using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderPromoSetMap : ClassMap<OnlineOrderPromoSet>
	{
		public OnlineOrderPromoSetMap()
		{
			Table("online_orders_promo_sets");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.ReceivedPromoSetId)
				.Column("received_promo_set_id")
				.Not.Nullable();
			
			Map(x => x.Count)
				.Column("count")
				.Not.Nullable();
			
			Map(x => x.Price)
				.Column("price")
				.Not.Nullable();

			References(x => x.OnlineOrder)
				.Column("online_order_id");
			
			References(x => x.PromoSet)
				.Column("promo_set_id");
			
			HasManyToMany(x => x.DiscountReasons)
				.Table("discount_reasons_online_orders_promo_sets")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("online_order_promo_set_id");
		}
	}
}
