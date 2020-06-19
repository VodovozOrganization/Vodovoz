using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы без отгрузки на постоплату",
		Nominative = "заказ без отгрузки на постоплату",
		Prepositional = "заказе без отгрузки на постоплату",
		PrepositionalPlural = "заказах без отгрузки на постоплату")]
	public class OrderWithoutShipmentForPayment : OrderWithoutShipmentBase
	{
		IList<OrderWithoutShipmentForPaymentItem> orderWithoutDeliveryForPaymentItems = new List<OrderWithoutShipmentForPaymentItem>();
		[Display(Name = "Строки заказа без отгрузки на постоплату")]
		public virtual IList<OrderWithoutShipmentForPaymentItem> OrderWithoutDeliveryForPaymentItems {
			get => orderWithoutDeliveryForPaymentItems;
			set => SetField(ref orderWithoutDeliveryForPaymentItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForPaymentItem> observableOrderWithoutDeliveryForPaymentItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForPaymentItem> ObservableOrderWithoutDeliveryForPaymentItems {
			get {
				if(observableOrderWithoutDeliveryForPaymentItems == null) {
					observableOrderWithoutDeliveryForPaymentItems = new GenericObservableList<OrderWithoutShipmentForPaymentItem>(orderWithoutDeliveryForPaymentItems);
				}

				return observableOrderWithoutDeliveryForPaymentItems;
			}
		}

		public OrderWithoutShipmentForPayment() { }
	}
}
