using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveryDiscussionCommentFileInformationMap : ClassMap<UndeliveryDiscussionCommentFileInformation>
	{
		public UndeliveryDiscussionCommentFileInformationMap()
		{
			Table("undelivery_discussion_comment_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.UndeliveryDiscussionCommentId)
				.Column("undelivery_discussion_comment_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
