using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PromotionalSetItemMap : ClassMap<PromotionalSetItem>
	{
		public PromotionalSetItemMap()
		{
			Table("promotional_set_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count");
			Map(x => x.Discount).Column("discount");
			Map(x => x.DiscountMoney).Column("discount_money");
			Map(x => x.IsDiscountInMoney).Column("is_discount_in_money");

			References(x => x.PromoSet).Column("promotional_set_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
