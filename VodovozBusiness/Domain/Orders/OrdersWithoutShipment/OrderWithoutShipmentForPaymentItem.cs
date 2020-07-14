using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки счета без отгрузки на постоплату",
		Nominative = "строка счета без отгрузки на постоплату")]
	public class OrderWithoutShipmentForPaymentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		OrderWithoutShipmentForPayment orderWithoutDeliveryForPayment;
		[Display(Name = "Счет без отгрузки на постоплату")]
		public virtual OrderWithoutShipmentForPayment OrderWithoutDeliveryForPayment {
			get => orderWithoutDeliveryForPayment;
			set => SetField(ref orderWithoutDeliveryForPayment, value);
		}

		Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		public OrderWithoutShipmentForPaymentItem() { }
	}
}
