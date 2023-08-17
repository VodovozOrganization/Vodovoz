using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PromotionalSetActionBaseMap : ClassMap<PromotionalSetActionBase>
	{
		public PromotionalSetActionBaseMap()
		{
			Table("promotional_set_actions");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("promotional_set_action_type");

			References(x => x.PromotionalSet).Column("promotional_set_id");
		}
	}

	public class PromotionalSetActionFixPriceMap : SubclassMap<PromotionalSetActionFixPrice>
	{
		public PromotionalSetActionFixPriceMap()
		{
			DiscriminatorValue(PromotionalSetActionType.FixedPrice.ToString());

			Map(x => x.Price).Column("price");
			Map(x => x.IsForZeroDebt).Column("is_for_zero_debt");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
