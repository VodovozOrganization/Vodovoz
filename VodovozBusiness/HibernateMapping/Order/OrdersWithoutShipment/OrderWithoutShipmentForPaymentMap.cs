using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.HibernateMapping.Order.OrdersWithoutShipment
{
    public class OrderWithoutShipmentForPaymentMap : ClassMap<OrderWithoutShipmentForPayment>
    {
        public OrderWithoutShipmentForPaymentMap()
        {
            Table("bills_without_shipment_for_payment");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.CreateDate).Column("create_date");

            References(x => x.Author).Column("author_id");
            References(x => x.Client).Column("client_id");
        }
    }
}