﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
	public class OrderItemMap : ClassMap<OrderItem>
	{
		public OrderItemMap ()
		{
			Table ("order_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.ActualCount)		.Column ("actual_count");
			Map (x => x.Count)				.Column ("count");
			Map (x => x.IsDiscountInMoney)	.Column ("is_discount_in_money");
			Map (x => x.Discount)			.Column ("discount");
			Map (x => x.DiscountMoney)		.Column ("discount_money");
			Map (x => x.IncludeNDS)			.Column ("include_nds");
			Map (x => x.Price)				.Column ("price");
			Map (x => x.IsUserPrice)		.Column ("is_user_price");

			References (x => x.AdditionalAgreement)			 .Column ("additional_agreement_id").Cascade.All();
			References (x => x.CounterpartyMovementOperation).Column ("counterparty_movement_operation_id").Cascade.All();
			References (x => x.Equipment)					 .Column ("equipment_id");
			References (x => x.Nomenclature)				 .Column ("nomenclature_id");
			References (x => x.Order)						 .Column ("order_id");
			References (x => x.FreeRentEquipment)			 .Column ("free_rent_equipment_id").Cascade.All();
			References (x => x.PaidRentEquipment)			 .Column ("paid_rent_equipment_id").Cascade.All();
			References (x => x.DiscountReason)			 	 .Column ("discount_reason_id").Cascade.All();
		}
	}
}