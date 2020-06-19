using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы без отгрузки на предоплату",
		Nominative = "заказ без отгрузки на предоплату",
		Prepositional = "заказе без отгрузки на предоплату",
		PrepositionalPlural = "заказах без отгрузки на предоплату")]
	public class OrderWithoutShipmentForAdvancePayment : OrderWithoutShipmentBase
	{
		IList<OrderWithoutShipmentForAdvancePaymentItem> orderWithoutDeliveryForAdvancePaymentItems = new List<OrderWithoutShipmentForAdvancePaymentItem>();
		[Display(Name = "Строки заказа без отгрузки на предоплату")]
		public virtual IList<OrderWithoutShipmentForAdvancePaymentItem> OrderWithoutDeliveryForAdvancePaymentItems {
			get => orderWithoutDeliveryForAdvancePaymentItems;
			set => SetField(ref orderWithoutDeliveryForAdvancePaymentItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem> observableOrderWithoutDeliveryForAdvancePaymentItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem> ObservableOrderWithoutDeliveryForAdvancePaymentItems {
			get {
				if(observableOrderWithoutDeliveryForAdvancePaymentItems == null) {
					observableOrderWithoutDeliveryForAdvancePaymentItems = new GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem>(orderWithoutDeliveryForAdvancePaymentItems);
				}

				return observableOrderWithoutDeliveryForAdvancePaymentItems;
			}
		}

		public virtual void RecalculateItemsPrice()
		{
			foreach(OrderWithoutShipmentForAdvancePaymentItem item in ObservableOrderWithoutDeliveryForAdvancePaymentItems) {
				if(item.Nomenclature.Category == NomenclatureCategory.water) {
					item.RecalculatePrice();
				}
			}
		}

		public virtual int GetTotalWater19LCount(bool doNotCountWaterFromPromoSets = false)
		{
			var water19L = ObservableOrderWithoutDeliveryForAdvancePaymentItems.Where(x => x.Nomenclature.IsWater19L);
			if(doNotCountWaterFromPromoSets)
				water19L = water19L.Where(x => x.PromoSet == null);
			return water19L.Sum(x => x.Count);
		}

		public OrderWithoutShipmentForAdvancePayment() { }
	}
}
