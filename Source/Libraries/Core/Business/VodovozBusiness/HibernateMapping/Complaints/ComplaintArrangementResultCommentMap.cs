using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintArrangementResultCommentMap : ClassMap<ComplaintArrangementResultComment>
	{
		public ComplaintArrangementResultCommentMap()
		{
			Table("complaint_arrangement_result_comments");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Complaint).Column("complaint_id");
			References(x => x.Author).Column("author_id");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CommentType).Column("type").CustomType<ComplaintTypeStringType>();
			Map(x => x.CreationTime).Column("creation_time");
		}
	}
}
