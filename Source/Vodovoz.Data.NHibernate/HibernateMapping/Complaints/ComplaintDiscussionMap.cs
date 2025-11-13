using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintDiscussionMap : ClassMap<ComplaintDiscussion>
	{
		public ComplaintDiscussionMap()
		{
			Table("complaint_discussions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Complaint).Column("complaint_id");
			References(x => x.Subdivision).Column("subdivision_id");
			Map(x => x.StartSubdivisionDate).Column("start_subdivision_date");
			Map(x => x.PlannedCompletionDate).Column("planned_completion_date");
			Map(x => x.Status).Column("status");

			HasMany(x => x.Comments).Cascade.All().Inverse().LazyLoad().KeyColumn("complaint_discussion_id");
		}
	}
}
