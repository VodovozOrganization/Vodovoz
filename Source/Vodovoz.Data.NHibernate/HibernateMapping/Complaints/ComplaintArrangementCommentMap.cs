using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintArrangementCommentMap : ClassMap<ComplaintArrangementComment>
	{
		public ComplaintArrangementCommentMap()
		{
			Table("complaint_arrangement_comments");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Complaint).Column("complaint_id");
			References(x => x.Author).Column("author_id");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationTime).Column("creation_time");
		}
	}
}
