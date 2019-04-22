using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class PromotionalSetItemMap : ClassMap<PromotionalSetItem>
	{
		public PromotionalSetItemMap()
		{
			Table("promotional_set_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count");
			Map(x => x.Discount).Column("discount");
			References(x => x.PromoSet).Column("promotional_set_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
