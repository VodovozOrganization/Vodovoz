using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Operations;
using System;
using QS.HistoryLog;

namespace Vodovoz.Domain.Payments
{
	[HistoryTrace]
	public class PaymentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		Payment payment;
		[Display(Name = "Платеж")]
		public virtual Payment Payment {
			get => payment;
			set => SetField(ref payment, value);
		}

		CashlessMovementOperation cashlessMovementOperation;

		public virtual CashlessMovementOperation CashlessMovementOperation {
			get => cashlessMovementOperation;
			set => SetField(ref cashlessMovementOperation, value);
		}

		decimal sum;

		public virtual decimal Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		public PaymentItem()
		{
		}

		public virtual void CreateExpenseOperation()
		{
			var expenseOperation = new CashlessMovementOperation { Expense = sum, OperationTime = DateTime.Now };
			cashlessMovementOperation = expenseOperation;
		}
	}
}
