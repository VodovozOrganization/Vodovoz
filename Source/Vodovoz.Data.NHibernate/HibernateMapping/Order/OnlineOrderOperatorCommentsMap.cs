using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderOperatorCommentsMap : ClassMap<OnlineOrderOperatorComments>
	{
		public OnlineOrderOperatorCommentsMap()
		{
			Table("online_order_operator_comments");
			OptimisticLock.Version();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
				
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreateTime).Column("create_time");
			
			References(x => x.CommentAuthor).Column("comment_author_id");
			References(x => x.OnlineOrder).Column("online_order_id");
		}
	}
}
