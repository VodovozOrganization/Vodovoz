using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderItemMap : ClassMap<OrderItem>
	{
		public OrderItemMap()
		{
			Table("order_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ActualCount).Column("actual_count");
			Map(x => x.Count).Column("count");
			Map(x => x.IsDiscountInMoney).Column("is_discount_in_money");
			Map(x => x.Discount).Column("discount");
			Map(x => x.OriginalDiscount).Column("original_discount");
			Map(x => x.DiscountMoney).Column("discount_money");
			Map(x => x.OriginalDiscountMoney).Column("original_discount_money");
			Map(x => x.DiscountByStock).Column("discount_by_stock");
			Map(x => x.IncludeNDS).Column("include_nds");
			Map(x => x.Price).Column("price");
			Map(x => x.IsUserPrice).Column("is_user_price");
			Map(x => x.ValueAddedTax).Column("value_added_tax");
			Map(x => x.RentType).Column("rent_type");
			Map(x => x.OrderItemRentSubType).Column("rent_sub_type");
			Map(x => x.RentCount).Column("rent_count");
			Map(x => x.RentEquipmentCount).Column("rent_equipment_count");
			Map(x => x.IsAlternativePrice).Column("is_alternative_price");
			Map(x => x.RecomendationId).Column("recomendation_id");

			References(x => x.CounterpartyMovementOperation).Column("counterparty_movement_operation_id").Cascade.All();
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Order).Column("order_id");
			References(x => x.DiscountReason).Column("discount_reason_id");
			References(x => x.OriginalDiscountReason).Column("original_discount_reason_id");
			References(x => x.PromoSet).Column("promotional_set_id");
			References(x => x.PaidRentPackage).Column("paid_rent_package_id");
			References(x => x.FreeRentPackage).Column("free_rent_package_id");
			References(x => x.CopiedFromUndelivery).Column("copied_from_undelivery_id");
		}
	}
}
