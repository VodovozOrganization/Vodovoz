using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OrderItemMap : ClassMap<OrderItemEntity>
	{
		public OrderItemMap()
		{
			Table("order_items");
			Not.LazyLoad();

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.ActualCount)
				.Column("actual_count");

			Map(x => x.Count)
				.Column("count");

			Map(x => x.IsDiscountInMoney)
				.Column("is_discount_in_money");

			Map(x => x.Discount)
				.Column("discount");

			Map(x => x.OriginalDiscount)
				.Column("original_discount");

			Map(x => x.DiscountMoney)
				.Column("discount_money");

			Map(x => x.OriginalDiscountMoney)
				.Column("original_discount_money");

			Map(x => x.DiscountByStock)
				.Column("discount_by_stock");

			Map(x => x.IncludeNDS)
				.Column("include_nds");

			Map(x => x.Price)
				.Column("price");

			Map(x => x.IsUserPrice)
				.Column("is_user_price");

			Map(x => x.ValueAddedTax)
				.Column("value_added_tax");

			Map(x => x.RentType)
				.Column("rent_type");

			Map(x => x.OrderItemRentSubType)
				.Column("rent_sub_type");

			Map(x => x.RentCount)
				.Column("rent_count");

			Map(x => x.RentEquipmentCount)
				.Column("rent_equipment_count");

			Map(x => x.IsAlternativePrice)
				.Column("is_alternative_price");


			References(x => x.Order)
				.Column("order_id");

			References(x => x.CopiedFromUndelivery)
				.Column("copied_from_undelivery_id");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");
		}
	}
}
