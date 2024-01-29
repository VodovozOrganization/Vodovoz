using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderItemMap : ClassMap<OnlineOrderItem>
	{
		public OnlineOrderItemMap()
		{
			Table("online_order_items");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.DiscountReasonId).Column("discount_reason_id");
			Map(x => x.PromoSetId).Column("promo_set_id");
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			Map(x => x.IsDiscountInMoney).Column("is_discount_money");
			Map(x => x.Discount).Column("discount");

			References(x => x.OnlineOrder).Column("online_order_id");
		}
	}
}
