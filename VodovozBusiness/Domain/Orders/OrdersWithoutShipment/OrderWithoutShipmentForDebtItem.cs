using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заказа без отгрузки на долг",
		Nominative = "строка заказа без отгрузки на долг")]
	public class OrderWithoutShipmentForDebtItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		OrderWithoutShipmentForDebt orderWithoutDeliveryForDebt;
		[Display(Name = "Заказ без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutDeliveryForDebt {
			get => orderWithoutDeliveryForDebt;
			set => SetField(ref orderWithoutDeliveryForDebt, value);
		}

		private decimal debtSum;
		[Display(Name = "Сумма долга")]
		public virtual decimal DebtSum {
			get => debtSum;
			set => SetField(ref debtSum, value);
		}

		private OrderWithoutShipmentForDebtItemType itemType =
			OrderWithoutShipmentForDebtItemType.ReconciliationArrears;
		[Display(Name = "Задолженность по акту сверки")]
		public virtual OrderWithoutShipmentForDebtItemType ItemType {
			get => itemType;
			set => SetField(ref itemType, value);
		}

		public OrderWithoutShipmentForDebtItem() { }
	}

	public enum OrderWithoutShipmentForDebtItemType
	{
		[Display(Name = "Задолженность по акту сверки")]
		ReconciliationArrears
	}
}
