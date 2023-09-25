using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.PromotionalSets;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PromotionalSetMap : ClassMap<PromotionalSet>
	{
		public PromotionalSetMap()
		{
			Table("promotional_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.DiscountReasonInfo).Column("discount_reason_info");
			Map(x => x.CanEditNomenclatureCount).Column("can_edit_nomenclature_count");
			Map(x => x.CanBeAddedWithOtherPromoSets).Column("can_be_added_with_other_promosets");
			Map(x => x.CanBeReorderedWithoutRestriction).Column("can_be_reordered_without_restriction");
			HasMany(x => x.PromotionalSetItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
			HasManyToMany(x => x.Orders).Table("promotional_sets_to_orders")
								.ParentKeyColumn("promotional_set_id")
								.ChildKeyColumn("order_id")
								.LazyLoad();
			HasMany(x => x.PromotionalSetActions).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
		}
	}
}
