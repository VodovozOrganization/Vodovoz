using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "действия с документами эдо и честный знак",
		Nominative = "действия с документом это и честный знак"
	)]
	[HistoryTrace]
	public class OrderEdoTrueMarkDocumentsActions : PropertyChangedBase, IDomainObject
	{
		private Order _order;
		private bool _isNeedToResendEdoUpd;
		private bool _isNeedToCancelTrueMarkDocument;
		private bool _isNeedToResendEdoBill;
		private OrderWithoutShipmentForAdvancePayment _orderWithoutShipmentForAdvancePayment;
		private OrderWithoutShipmentForDebt _orderWithoutShipmentForDebt;
		private OrderWithoutShipmentForPayment _orderWithoutShipmentForPayment;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Счёт без отгрузки на постоплату")]
		public OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}

		[Display(Name = "Счёт без отгрузки на предоплату")]
		public OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}

		[Display(Name = "Счёт без отгрузки на долг")]
		public OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}

		[Display(Name = "Требуется переотправка УПД по ЭДО")]
		public bool IsNeedToResendEdoUpd
		{
			get => _isNeedToResendEdoUpd;
			set => SetField(ref _isNeedToResendEdoUpd, value);
		}

		[Display(Name = "Требуется переотправка счёта по ЭДО")]
		public bool IsNeedToResendEdoBill
		{
			get => _isNeedToResendEdoBill;
			set => SetField(ref _isNeedToResendEdoBill, value);
		}

		[Display(Name = "Требуется отмена вывода из оборота в Честном Знаке")]
		public bool IsNeedToCancelTrueMarkDocument
		{
			get => _isNeedToCancelTrueMarkDocument;
			set => SetField(ref _isNeedToCancelTrueMarkDocument, value);
		}

		public virtual string Title => $"Действия с документами ЭДО и Честный знак заказа №" +
			$"{Order?.Id ?? OrderWithoutShipmentForDebt?.Id ?? OrderWithoutShipmentForPayment?.Id ?? OrderWithoutShipmentForAdvancePayment?.Id}";
	}
}
