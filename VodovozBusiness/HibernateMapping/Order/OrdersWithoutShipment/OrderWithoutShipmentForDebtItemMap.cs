using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.HibernateMapping.Order.OrdersWithoutShipment
{
    public class OrderWithoutShipmentForDebtItemMap :ClassMap<OrderWithoutShipmentForDebtItem>
    {
        public OrderWithoutShipmentForDebtItemMap()
        {
            Table("order_without_delivery_for_debt_items");
            
            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.DebtSum).Column("debt_sum");
            References (x => x.OrderWithoutDeliveryForDebt).Column ("order_without_delivery_for_debt_id");
        }
    }
}