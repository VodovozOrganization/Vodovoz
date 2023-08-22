using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class DeliveryPriceRuleMap : ClassMap<DeliveryPriceRule>
	{
		public DeliveryPriceRuleMap()
		{
			Table("delivery_price_rules");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Water19LCount).Column("wtr_19L_qty");
			Map(x => x.Water6LCount).Column("wtr_6L_qty");
			Map(x => x.Water1500mlCount).Column("wtr_1L500ml_qty");
			Map(x => x.Water600mlCount).Column("wtr_600ml_qty");
			Map(x => x.Water500mlCount).Column("wtr_500ml_qty");
			Map(x => x.OrderMinSumEShopGoods).Column("order_min_sum_eshop_goods");
			Map(x => x.RuleName).Column("rule_name");
		}
	}
}
