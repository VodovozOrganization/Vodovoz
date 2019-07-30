using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintDiscussionCommentMap : ClassMap<ComplaintDiscussionComment>
	{
		public ComplaintDiscussionCommentMap()
		{
			Table("complaint_discussion_comments");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ComplaintDiscussion).Column("complaint_discussion_id");
			Map(x => x.Comment).Column("comment");
		}
	}
}
