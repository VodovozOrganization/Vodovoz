using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class DistrictsSetMap : ClassMap<DistrictsSet>
	{
		public DistrictsSetMap()
		{
			Table("districts_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.DateCreated).Column("date_created");
			Map(x => x.DateActivated).Column("date_activated");
			Map(x => x.DateClosed).Column("date_closed");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Status).Column("status");
			Map(x => x.OnlineStoreOrderSumForFreeDelivery).Column("online_store_order_sum_for_free_delivery");

			References(x => x.Author).Column("author_id");

			HasMany(x => x.Districts).Cascade.AllDeleteOrphan().Inverse().KeyColumn("districts_set_id");
		}
	}
}
