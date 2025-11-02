using System.ComponentModel.DataAnnotations;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Строки платежа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки платежа(куда распределены)",
		Nominative = "строка платежа(куда распределен)",
		Prepositional = "строке платежа(куда распределен)",
		PrepositionalPlural = "строках платежа(куда распределены)")]
	public class PaymentItemEntity : PropertyChangedBase, IDomainObject
	{
		private decimal _sum;
		private AllocationStatus _paymentItemStatus;
		private OrderEntity _order;
		private PaymentEntity _payment;
		private CashlessMovementOperationEntity _cashlessMovementOperation;

		public virtual int Id { get; set; }

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
		/// Операция распределения по безналу
		/// </summary>
		[Display(Name = "Операция распределения по безналу")]
		public virtual CashlessMovementOperationEntity CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		/// <summary>
		/// Сумма распределения
		/// </summary>
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		/// <summary>
		/// Статус распределения
		/// </summary>
		public virtual AllocationStatus PaymentItemStatus
		{
			get => _paymentItemStatus;
			set => SetField(ref _paymentItemStatus, value);
		}
	}
}
