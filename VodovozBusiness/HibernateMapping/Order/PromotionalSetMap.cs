using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Orders
{
	public class PromotionalSetMap : ClassMap<PromotionalSet>
	{
		public PromotionalSetMap()
		{
			Table("promotional_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.IsArchive).Column("is_archive");
			References(x => x.PromoSetName).Column("discount_reason_id");
			HasMany(x => x.PromotionalSetItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("promotional_set_id");
		}
	}
}
