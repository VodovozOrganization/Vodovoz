using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Orders
{
	public class UndeliveryDiscussionMap : ClassMap<UndeliveryDiscussion>
	{
		public UndeliveryDiscussionMap()
		{
			Table("undelivery_discussions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Undelivery).Column("undelivery_id");
			References(x => x.Subdivision).Column("subdivision_id");
			Map(x => x.StartSubdivisionDate).Column("start_subdivision_date");
			Map(x => x.PlannedCompletionDate).Column("planned_completion_date");
			Map(x => x.Status).Column("status");

			HasMany(x => x.Comments).Cascade.All().Inverse().LazyLoad().KeyColumn("undelivery_discussion_id").OrderBy("creation_time desc");
		}
	}
}
