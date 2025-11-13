using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Действие с документом ЭДО и честного знака
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "действия с документами ЭДО и честного знака",
		Nominative = "действие с документом ЭДО и честного знака"
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
		private bool _isNeedOfferCancellation;
		private DateTime? _created;

		public OrderEdoTrueMarkDocumentsActions()
		{
			_created = DateTime.Now;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public DateTime? Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Счёт без отгрузки на постоплату
		/// </summary>
		[Display(Name = "Счёт без отгрузки на постоплату")]
		public OrderWithoutShipmentForPayment OrderWithoutShipmentForPayment
		{
			get => _orderWithoutShipmentForPayment;
			set => SetField(ref _orderWithoutShipmentForPayment, value);
		}

		/// <summary>
		/// Счёт без отгрузки на предоплату
		/// </summary>
		[Display(Name = "Счёт без отгрузки на предоплату")]
		public OrderWithoutShipmentForAdvancePayment OrderWithoutShipmentForAdvancePayment
		{
			get => _orderWithoutShipmentForAdvancePayment;
			set => SetField(ref _orderWithoutShipmentForAdvancePayment, value);
		}

		/// <summary>
		/// Счёт без отгрузки на долг
		/// </summary>
		[Display(Name = "Счёт без отгрузки на долг")]
		public OrderWithoutShipmentForDebt OrderWithoutShipmentForDebt
		{
			get => _orderWithoutShipmentForDebt;
			set => SetField(ref _orderWithoutShipmentForDebt, value);
		}

		/// <summary>
		/// Требуется переотправка УПД по ЭДО
		/// </summary>
		[Display(Name = "Требуется переотправка УПД по ЭДО")]
		public bool IsNeedToResendEdoUpd
		{
			get => _isNeedToResendEdoUpd;
			set => SetField(ref _isNeedToResendEdoUpd, value);
		}

		/// <summary>
		/// Требуется переотправка счёта по ЭДО
		/// </summary>
		[Display(Name = "Требуется переотправка счёта по ЭДО")]
		public bool IsNeedToResendEdoBill
		{
			get => _isNeedToResendEdoBill;
			set => SetField(ref _isNeedToResendEdoBill, value);
		}

		/// <summary>
		/// Требуется отмена вывода из оборота в Честном Знаке
		/// </summary>
		[Display(Name = "Требуется отмена вывода из оборота в Честном Знаке")]
		public bool IsNeedToCancelTrueMarkDocument
		{
			get => _isNeedToCancelTrueMarkDocument;
			set => SetField(ref _isNeedToCancelTrueMarkDocument, value);
		}

		/// <summary>
		/// Требуется отмена счёта
		/// </summary>
		[Display(Name = "Требуется отмена счёта")]
		public virtual bool IsNeedOfferCancellation
		{
			get => _isNeedOfferCancellation;
			set => SetField(ref _isNeedOfferCancellation, value);
		}

		/// <summary>
		/// Заголовок (вычисляемое)
		/// </summary>
		public virtual string Title => $"Действия с документами ЭДО и Честный знак заказа №" +
			$"{Order?.Id ?? OrderWithoutShipmentForDebt?.Id ?? OrderWithoutShipmentForPayment?.Id ?? OrderWithoutShipmentForAdvancePayment?.Id}";
	}
}
