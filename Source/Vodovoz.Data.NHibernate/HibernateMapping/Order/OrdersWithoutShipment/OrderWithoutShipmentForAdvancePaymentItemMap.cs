using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForAdvancePaymentItemMap : ClassMap<OrderWithoutShipmentForAdvancePaymentItem>
	{
		public OrderWithoutShipmentForAdvancePaymentItemMap()
		{
			Table("bill_without_shipment_for_advance_payment_items");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count");
			Map(x => x.IsDiscountInMoney).Column("is_discount_in_money");
			Map(x => x.Discount).Column("discount");
			Map(x => x.DiscountMoney).Column("discount_money");
			Map(x => x.DiscountByStock).Column("discount_by_stock");
			Map(x => x.IncludeNDS).Column("include_nds");
			Map(x => x.Price).Column("price");
			Map(x => x.IsUserPrice).Column("is_user_price");
			Map(x => x.ValueAddedTax).Column("value_added_tax");
			Map(x => x.IsAlternativePrice).Column("is_alternative_price");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.OrderWithoutDeliveryForAdvancePayment).Column("bill_ws_for_advance_payment_id");
			References(x => x.DiscountReason).Column("discount_reason_id");
		}
	}
}
