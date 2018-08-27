using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class UndeliveredOrderCommentsMap : ClassMap<UndeliveredOrderComment>
	{
		public UndeliveredOrderCommentsMap()
		{
			Table("undelivered_orders_comments");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CommentDate).Column("date_and_time");
			Map(x => x.Comment).Column("comment");

			Map(x => x.CommentedField).Column("field_name").CustomType<UndeliveredOrderCommentsCommentedFieldsStringType>();

			References(x => x.UndeliveredOrder).Column("undelivery_id");
			References(x => x.Employee).Column("employee_id");
		}
	}
}
