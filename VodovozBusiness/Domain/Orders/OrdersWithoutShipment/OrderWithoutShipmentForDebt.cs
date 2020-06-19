using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы без отгрузки на долги",
		Nominative = "заказ без отгрузки на долг",
		Prepositional = "заказе без отгрузки на долг",
		PrepositionalPlural = "заказах без отгрузки на долги")]
	public class OrderWithoutShipmentForDebt : OrderWithoutShipmentBase
	{
		IList<OrderWithoutShipmentForDebtItem> orderWithoutDeliveryForDebtItems = new List<OrderWithoutShipmentForDebtItem>();
		[Display(Name = "Строки заказа без отгрузки на долг")]
		public virtual IList<OrderWithoutShipmentForDebtItem> OrderWithoutDeliveryForDebtItems {
			get => orderWithoutDeliveryForDebtItems;
			set => SetField(ref orderWithoutDeliveryForDebtItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForDebtItem> observableOrderWithoutDeliveryForDebtItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForDebtItem> ObservableOrderWithoutDeliveryForDebtItems {
			get {
				if(observableOrderWithoutDeliveryForDebtItems == null) {
					observableOrderWithoutDeliveryForDebtItems = new GenericObservableList<OrderWithoutShipmentForDebtItem>(orderWithoutDeliveryForDebtItems);
				}

				return observableOrderWithoutDeliveryForDebtItems;
			}
		}

		public OrderWithoutShipmentForDebt() { }
	}
}
