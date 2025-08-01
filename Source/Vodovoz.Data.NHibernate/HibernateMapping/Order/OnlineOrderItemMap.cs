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
			
			Map(x => x.NomenclatureId).Column("first_nomenclature_id");
			Map(x => x.PromoSetId).Column("first_promo_set_id");
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			Map(x => x.IsDiscountInMoney).Column("is_discount_money");
			Map(x => x.IsFixedPrice).Column("is_fixed_price");
			Map(x => x.PercentDiscount).Column("percent_discount");
			Map(x => x.MoneyDiscount).Column("money_discount");
			Map(x => x.CountFromPromoSet).Column("count_from_promo_set");
			Map(x => x.NomenclaturePrice).Column("nomenclature_price");
			Map(x => x.DiscountFromPromoSet).Column("discount_from_promo_set");
			Map(x => x.IsDiscountInMoneyFromPromoSet).Column("is_discount_in_money_from_promo_set");

			References(x => x.OnlineOrder).Column("online_order_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.PromoSet).Column("promo_set_id");
			References(x => x.DiscountReason).Column("discount_reason_id");
		}
	}
}
