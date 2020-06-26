using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.HibernateMapping.Order.OrdersWithoutShipment
{
    public class OrderWithoutShipmentForAdvancePaymentItemMap : ClassMap<OrderWithoutShipmentForAdvancePaymentItem>
    {
        public OrderWithoutShipmentForAdvancePaymentItemMap()
        {
            Table ("order_items");
            Not.LazyLoad ();

            Id (x => x.Id).Column ("id").GeneratedBy.Native ();
            
            Map (x => x.Count).Column ("count");
            Map (x => x.IsDiscountInMoney).Column ("is_discount_in_money");
            Map (x => x.Discount).Column ("discount");
            Map (x => x.DiscountMoney).Column ("discount_money");
            Map	(x => x.DiscountByStock).Column ("discount_by_stock");
            Map (x => x.IncludeNDS).Column ("include_nds");
            Map (x => x.Price).Column ("price");
            Map (x => x.IsUserPrice).Column ("is_user_price");
            Map (x => x.ValueAddedTax).Column ("value_added_tax");
            
            References (x => x.Nomenclature).Column ("nomenclature_id");
            References (x => x.OrderWithoutDeliveryForAdvancePayment).Column ("order_ws_for_advance_payment_id");
            //References (x => x.FreeRentEquipment)			 .Column ("free_rent_equipment_id").Cascade.All();
            //References (x => x.PaidRentEquipment)			 .Column ("paid_rent_equipment_id").Cascade.All();
            References (x => x.DiscountReason).Column ("discount_reason_id");
        }
    }
}