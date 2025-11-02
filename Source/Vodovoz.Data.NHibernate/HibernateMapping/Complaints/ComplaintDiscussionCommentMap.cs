using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintDiscussionCommentMap : ClassMap<ComplaintDiscussionComment>
	{
		public ComplaintDiscussionCommentMap()
		{
			Table("complaint_discussion_comments");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ComplaintDiscussion).Column("complaint_discussion_id");
			References(x => x.Author).Column("author_id");
			Map(x => x.CreationTime).Column("creation_time");
			Map(x => x.Comment).Column("comment");

			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("complaint_discussion_comment_id");
		}
	}
}
