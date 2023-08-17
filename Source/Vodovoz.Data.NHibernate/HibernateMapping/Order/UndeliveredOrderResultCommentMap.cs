using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class UndeliveredOrderResultCommentMap : ClassMap<UndeliveredOrderResultComment>
	{
		public UndeliveredOrderResultCommentMap()
		{
			Table("undelivered_orders_result_comments");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.UndeliveredOrder).Column("undelivered_order_id");
			References(x => x.Author).Column("author_id");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationTime).Column("creation_time");
		}
	}
}
