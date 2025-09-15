using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForPaymentMap : ClassMap<OrderWithoutShipmentForPayment>
	{
		public OrderWithoutShipmentForPaymentMap()
		{
			Table("bills_without_shipment_for_payment");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.IsBillWithoutShipmentSent).Column("is_bill_sent");

			References(x => x.Organization).Column("organization_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Client).Column("client_id");

			HasMany(x => x.OrderWithoutDeliveryForPaymentItems).Cascade
				.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("bill_ws_for_payment_id");
		}
	}
}
