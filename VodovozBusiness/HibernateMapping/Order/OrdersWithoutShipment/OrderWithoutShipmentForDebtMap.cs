using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.HibernateMapping.Order.OrdersWithoutShipment
{
    public class OrderWithoutShipmentForDebtMap : ClassMap<OrderWithoutShipmentForDebt>
    {
        public OrderWithoutShipmentForDebtMap()
        {
            Table("order_without_delivery_for_debt");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.CreateDate).Column("create_date");
            
            References(x => x.Author).Column("author_id");
            References(x => x.Client).Column("client_id");
            
            HasMany(x => x.OrderWithoutDeliveryForDebtItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn ("order_without_delivery_for_debt_id");
        }
    }
}