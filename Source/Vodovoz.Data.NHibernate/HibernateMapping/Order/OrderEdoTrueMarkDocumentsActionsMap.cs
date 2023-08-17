using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class OrderEdoTrueMarkDocumentsActionsMap : ClassMap<OrderEdoTrueMarkDocumentsActions>
	{
		public OrderEdoTrueMarkDocumentsActionsMap()
		{
			Table("order_edo_truemark_documents_actions");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Column("order_id");

			Map(x => x.IsNeedToResendEdoUpd).Column("is_need_to_recend_edo_upd");
			Map(x => x.IsNeedToCancelTrueMarkDocument).Column("is_need_to_cancel_truemark_doc");
		}
	}
}
