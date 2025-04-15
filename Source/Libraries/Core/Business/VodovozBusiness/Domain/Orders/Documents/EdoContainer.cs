using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.Orders.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Контейнеры электронного документооборота",
		Nominative = "Контейнер электронного документооборота",
		Prepositional = "Контейнере электронного документооборота",
		PrepositionalPlural = "Контейнерах электронного документооборота")]
	[HistoryTrace]
	public class EdoContainer : EdoContainerEntity
	{
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;
		private OrderWithoutShipmentForPayment _orderWithoutShipmentForPayment;

		[Display(Name = "Счет без отгрузки на предоплату")]
		public virtual OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}

		[Display(Name = "Cчет без отгрузки на долг")]
		public virtual OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}

		[Display(Name = "Cчет без отгрузки на постоплату")]
		public virtual OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}
	}
}
