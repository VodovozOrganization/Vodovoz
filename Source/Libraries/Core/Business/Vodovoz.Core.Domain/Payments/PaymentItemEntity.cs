using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Сущность строки платежа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки платежа(куда распределены)",
		Nominative = "строка платежа(куда распределен)",
		Prepositional = "строке платежа(куда распределен)",
		PrepositionalPlural = "строках платежа(куда распределены)")]
	[HistoryTrace]
	public class PaymentItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private decimal _sum;
		private AllocationStatus _paymentItemStatus;
		private OrderEntity _order;
		private PaymentEntity _payment;

		/// <summary>
		/// Номер строки заказа
		/// </summary>
		[Display(Name = "Номер строки заказа")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Платеж
		/// </summary>
		[Display(Name = "Платеж")]
		public virtual PaymentEntity Payment
		{
			get => _payment;
			set => SetField(ref _payment, value);
		}

		/// <summary>
		/// Сумма платежа
		/// </summary>
		[Display(Name = "Сумма платежа")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		/// <summary>
		/// Статус распределения платежа
		/// </summary>
		[Display(Name = "Статус распределения платежа")]
		public virtual AllocationStatus PaymentItemStatus
		{
			get => _paymentItemStatus;
			set => SetField(ref _paymentItemStatus, value);
		}
	}
}
