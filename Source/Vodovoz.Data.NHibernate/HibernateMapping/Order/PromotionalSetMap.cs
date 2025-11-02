using FluentNHibernate.Mapping;
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
			Map(x => x.OnlineName).Column("online_name");
			Map(x => x.PromotionalSetForNewClients).Column("for_new_clients");
			Map(x => x.BottlesCountForCalculatingDeliveryPrice)
				.Column("bottles_count_for_calculating_delivery_price");

			HasMany(x => x.PromotionalSetItems)
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
			HasMany(x => x.PromotionalSetActions)
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
			HasMany(x => x.PromotionalSetOnlineParameters)
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
			
			HasManyToMany(x => x.Orders)
				.Table("promotional_sets_to_orders")
				.ParentKeyColumn("promotional_set_id")
				.ChildKeyColumn("order_id")
				.LazyLoad();
		}
	}
}
