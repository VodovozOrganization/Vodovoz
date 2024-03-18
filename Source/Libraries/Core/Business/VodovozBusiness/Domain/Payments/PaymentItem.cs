using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки платежа(куда распределены)",
		Nominative = "строка платежа(куда распределен)",
		Prepositional = "строке платежа(куда распределен)",
		PrepositionalPlural = "строках платежа(куда распределены)")]
	[HistoryTrace]
	public class PaymentItem : PropertyChangedBase, IDomainObject
	{
		private decimal _sum;
		private AllocationStatus _paymentItemStatus;
		private Order _order;
		private Payment _payment;
		private CashlessMovementOperation _cashlessMovementOperation;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Платеж")]
		public virtual Payment Payment
		{
			get => _payment;
			set => SetField(ref _payment, value);
		}

		[Display(Name = "Операция распределения по безналу")]
		public virtual CashlessMovementOperation CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		public virtual AllocationStatus PaymentItemStatus
		{
			get => _paymentItemStatus;
			set => SetField(ref _paymentItemStatus, value);
		}

		public virtual void CreateOrUpdateExpenseOperation()
		{
			if(CashlessMovementOperation == null)
			{
				CashlessMovementOperation = new CashlessMovementOperation
				{
					Expense = _sum,
					Counterparty = Payment.Counterparty,
					Organization = Payment.Organization,
					OperationTime = DateTime.Now,
					CashlessMovementOperationStatus = AllocationStatus.Accepted
				};
			}
			else
			{
				UpdateExpenseOperation();
			}
		}

		public virtual void UpdateExpenseOperation()
		{
			if(CashlessMovementOperation.Expense != _sum)
			{
				CashlessMovementOperation.Expense = _sum;
				CashlessMovementOperation.Counterparty = Payment.Counterparty;
				CashlessMovementOperation.Organization = Payment.Organization;
				CashlessMovementOperation.OperationTime = DateTime.Now;
			}
		}

		public virtual void UpdateSum(decimal newSum)
		{
			Sum = newSum;

			if(CashlessMovementOperation != null)
			{
				UpdateExpenseOperation();
			}
		}

		public virtual void CancelAllocation(bool needUpdateOrderPaymentStatus = false)
		{
			UpdateStatuses(AllocationStatus.Cancelled);

			if(needUpdateOrderPaymentStatus)
			{
				Order.UpdateOrderPaymentStatus(Sum);
			}
		}

		public virtual void ReturnFromCancelled()
		{
			UpdateStatuses(AllocationStatus.Accepted);
		}

		private void UpdateStatuses(AllocationStatus status)
		{
			PaymentItemStatus = status;

			if(CashlessMovementOperation != null)
			{
				CashlessMovementOperation.CashlessMovementOperationStatus = status;
			}
		}
	}
}
