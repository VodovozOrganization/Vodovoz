using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintDiscussionCommentFileInformationMap
		: ClassMap<ComplaintDiscussionCommentFileInformation>
	{
		public ComplaintDiscussionCommentFileInformationMap()
		{
			Table("complaint_discussion_comment_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ComplaintDiscussionCommentId)
				.Column("complaint_discussion_comment_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
