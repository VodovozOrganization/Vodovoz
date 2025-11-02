using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderEdoTrueMarkDocumentsActionsMap : ClassMap<OrderEdoTrueMarkDocumentsActions>
	{
		public OrderEdoTrueMarkDocumentsActionsMap()
		{
			Table("order_edo_truemark_documents_actions");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Column("order_id");
			References(x => x.OrderWithoutShipmentForAdvancePayment).Column("order_without_shipment_for_advance_payment_id");
			References(x => x.OrderWithoutShipmentForDebt).Column("order_without_shipment_for_debt_id");
			References(x => x.OrderWithoutShipmentForPayment).Column("order_without_shipment_for_payment_id");

			Map(x => x.IsNeedToResendEdoUpd).Column("is_need_to_recend_edo_upd");
			Map(x => x.IsNeedToResendEdoBill).Column("is_need_to_recend_edo_bill");
			Map(x => x.IsNeedToCancelTrueMarkDocument).Column("is_need_to_cancel_truemark_doc");
			Map(x => x.IsNeedOfferCancellation).Column("is_need_offer_cancellation");
			Map(x => x.Created).Column("created");
		}
	}
}
