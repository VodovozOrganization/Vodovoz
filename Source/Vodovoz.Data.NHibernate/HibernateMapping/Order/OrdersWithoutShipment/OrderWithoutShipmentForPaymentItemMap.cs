using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.HibernateMapping.Order.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentItemMap : ClassMap<OrderWithoutShipmentForPaymentItem>
	{
		public OrderWithoutShipmentForPaymentItemMap()
		{
			Table("bill_without_shipment_for_payment_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Column("order_id");
			References(x => x.OrderWithoutDeliveryForPayment).Column("bill_ws_for_payment_id");
		}
	}
}