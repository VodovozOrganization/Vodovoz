using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Operations;

namespace Vodovoz.Domain.Payments
{
	/// <summary>
	/// Строка платежа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки платежа(куда распределены)",
		Nominative = "строка платежа(куда распределен)",
		Prepositional = "строке платежа(куда распределен)",
		PrepositionalPlural = "строках платежа(куда распределены)")]
	[HistoryTrace]
	public class PaymentItem : PaymentItemEntity
	{
		private Order _order;
		private Payment _payment;
		private CashlessMovementOperation _cashlessMovementOperation;

		[Display(Name = "Заказ")]
		public virtual new Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Платеж")]
		public virtual new Payment Payment
		{
			get => _payment;
			set => SetField(ref _payment, value);
		}

		[Display(Name = "Операция распределения по безналу")]
		public virtual new CashlessMovementOperation CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		/// <summary>
		/// Создание или обновление операции списания
		/// </summary>
		public virtual void CreateOrUpdateExpenseOperation()
		{
			if(CashlessMovementOperation == null)
			{
				CashlessMovementOperation = new CashlessMovementOperation
				{
					Expense = Sum,
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

		/// <summary>
		/// Обновление операции списания
		/// </summary>
		public virtual void UpdateExpenseOperation()
		{
			if(CashlessMovementOperation.Expense != Sum)
			{
				CashlessMovementOperation.Expense = Sum;
				CashlessMovementOperation.Counterparty = Payment.Counterparty;
				CashlessMovementOperation.Organization = Payment.Organization;
				CashlessMovementOperation.OperationTime = DateTime.Now;
			}
		}

		/// <summary>
		/// Обновление суммы
		/// </summary>
		/// <param name="newSum">Новая сумма</param>
		public virtual void UpdateSum(decimal newSum)
		{
			Sum = newSum;

			if(CashlessMovementOperation != null)
			{
				UpdateExpenseOperation();
			}
		}

		/// <summary>
		/// Отмена распределения
		/// </summary>
		/// <param name="needUpdateOrderPaymentStatus">Нужно обновлять статус оплаты заказа</param>
		public virtual void CancelAllocation()
		{
			UpdateStatuses(AllocationStatus.Cancelled);
		}

		/// <summary>
		/// Возврат статуса из отмены
		/// </summary>
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
