using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Orders
{
	public class UndeliveryDiscussionCommentMap : ClassMap<UndeliveryDiscussionComment>
	{
		public UndeliveryDiscussionCommentMap()
		{
			Table("undelivery_discussion_comments");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.UndeliveryDiscussion).Column("undelivery_discussion_id");
			References(x => x.Author).Column("author_id");
			Map(x => x.CreationTime).Column("creation_time").ReadOnly();
			Map(x => x.Comment).Column("comment");
			
			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("undelivery_discussion_comment_id");
		}
	}
}
