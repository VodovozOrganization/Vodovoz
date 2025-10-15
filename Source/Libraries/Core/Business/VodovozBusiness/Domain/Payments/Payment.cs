using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Operations;

namespace Vodovoz.Domain.Payments
{
	/// <summary>
	/// Платеж
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи",
		Nominative = "платёж",
		Prepositional = "платеже",
		PrepositionalPlural = "платежах")]
	[HistoryTrace]
	[EntityPermission]
	public class Payment : PaymentEntity, IValidatableObject
	{
		private const int _commentLimit = 300;
		private const int _paymentPurposeLimit = 300;
		
		private CashlessMovementOperation _cashlessMovementOperation;
		private Counterparty _counterparty;
		private Organization _organization;
		private Payment _refundedPayment;
		private IObservableList<PaymentItem> _items = new ObservableList<PaymentItem>();

		public Payment() { }

		public Payment(TransferDocument doc, Organization org, Counterparty counterparty)
		{
			PaymentNum = int.Parse(doc.DocNum);
			Date = doc.ReceiptDate ?? doc.Date;
			Total = doc.Total;
			CounterpartyInn = doc.PayerInn;
			CounterpartyKpp = doc.PayerKpp;
			CounterpartyName = doc.PayerName;
			PaymentPurpose = doc.PaymentPurpose;
			CounterpartyBank = doc.PayerBank;
			CounterpartyAcc = doc.PayerAccount;
			CounterpartyCurrentAcc = doc.PayerCurrentAccount;
			CounterpartyCorrespondentAcc = doc.PayerCorrespondentAccount;
			CounterpartyBik = doc.PayerBik;

			if(org != null)
			{
				var account = string.IsNullOrWhiteSpace(doc.RecipientCurrentAccount) ? doc.RecipientAccount : doc.RecipientCurrentAccount;
				Organization = org;
				OrganizationAccount = org.Accounts.FirstOrDefault(acc => acc.Number == account);
			}

			if(counterparty != null)
			{
				Counterparty = counterparty;
				CounterpartyAccount = counterparty.Accounts.FirstOrDefault(acc => acc.Number == doc.PayerCurrentAccount);
			}
		}

		/// <summary>
		/// Строки платежа
		/// </summary>
		public virtual new IObservableList<PaymentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Операция передвижения безнала
		/// </summary>
		[Display(Name = "Операция передвижения безнала")]
		public virtual new CashlessMovementOperation CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual new Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Возвращаемый платеж
		/// </summary>
		[Display(Name = "Возвращаемый платеж")]
		public virtual new Payment RefundedPayment
		{
			get => _refundedPayment;
			set => SetField(ref _refundedPayment, value);
		}

		/// <summary>
		/// Возврат оплаты на счет, в случае отмены оплаченного заказа
		/// </summary>
		public virtual bool IsRefundPayment => RefundedPayment != null;

		/// <summary>
		/// Добавить строку платежа
		/// </summary>
		/// <param name="order">Заказ, на который распределяется оплата</param>
		public virtual void AddPaymentItem(Order order)
		{
			var paymentItem = new PaymentItem
			{
				Order = order,
				Payment = this,
				Sum = order.OrderSum,
				PaymentItemStatus = AllocationStatus.Accepted
			};

			Items.Add(paymentItem);
		}

		/// <summary>
		/// Добавить строку платежа
		/// </summary>
		/// <param name="order">Заказ, на который распределяется оплата</param>
		/// <param name="sum">Сумма распределения</param>
		public virtual void AddPaymentItem(Order order, decimal sum)
		{
			var item = Items.SingleOrDefault(x =>
				x.Order.Id == order.Id && x.PaymentItemStatus != AllocationStatus.Cancelled);

			if(item == null)
			{
				var paymentItem = new PaymentItem
				{
					Order = order,
					Payment = this,
					Sum = sum,
					PaymentItemStatus = AllocationStatus.Accepted
				};

				Items.Add(paymentItem);
			}
			else
			{
				item.Sum += sum;
			}
		}

		/// <summary>
		/// Удалить строку платежа
		/// </summary>
		/// <param name="paymentItemId">Id удаляемой строки</param>
		public virtual void RemovePaymentItem(int paymentItemId)
		{
			var paymentItem = Items.SingleOrDefault(pi => pi.Id == paymentItemId);

			if(paymentItem != null)
			{
				Items.Remove(paymentItem);
			}
		}

		/// <summary>
		/// Создать операцию прихода
		/// </summary>
		/// <returns><c>true</c> - успешно, <c>false</c> - нет</returns>
		public virtual bool CreateIncomeOperation()
		{
			if(CashlessMovementOperation != null || IsRefundPayment)
			{
				return false;
			}

			CashlessMovementOperation = new CashlessMovementOperation
			{
				Income = Total,
				Counterparty = Counterparty,
				Organization = Organization,
				OperationTime = DateTime.Now,
				CashlessMovementOperationStatus = AllocationStatus.Accepted
			};

			return true;
		}

		/// <summary>
		/// Создание платежа для возврата суммы на счет клиента
		/// </summary>
		/// <param name="paymentSum">Сумма возврата</param>
		/// <param name="orderId">Id заказа</param>
		/// <param name="refundPaymentReason">Причина возврата</param>
		/// <returns>Созданный платеж</returns>
		public virtual Payment CreatePaymentForReturnAllocatedSumToClientBalance(
			decimal paymentSum,
			int orderId,
			RefundPaymentReason refundPaymentReason)
		{
			return new Payment
			{
				PaymentNum = PaymentNum,
				Date = DateTime.Now,
				Total = paymentSum,
				ProfitCategory = ProfitCategory,
				PaymentPurpose = $"Возврат суммы оплаты заказа №{orderId} на баланс клиента. Причина: {refundPaymentReason.GetEnumTitle()}",
				Organization = Organization,
				Counterparty = Counterparty,
				CounterpartyName = CounterpartyName,
				Status = PaymentState.undistributed,
				RefundedPayment = this,
				RefundPaymentFromOrderId = orderId
			};
		}

		/// <summary>
		/// Заполнение данных плательщика
		/// </summary>
		public virtual void FillPropertiesFromCounterparty()
		{
			CounterpartyInn = Counterparty.INN;
			CounterpartyKpp = Counterparty.KPP;
			CounterpartyName = Counterparty.Name;
		}
		
		/// <summary>
		/// Платеж не для распределения(платежи межу нашими компаниями, приходы от физ лиц)
		/// </summary>
		public virtual void OtherIncome(ProfitCategory profitCategory)
		{
			ProfitCategory = profitCategory;
			Status = PaymentState.undistributed;
		}

		/// <summary>
		/// Отмена распределения
		/// </summary>
		/// <param name="cancellationReason">Причина отмены</param>
		/// <param name="needUpdateOrderPaymentStatus">Необходимость обновления статуса оплаты заказа</param>
		/// <param name="isByUserRequest">Пользовательский запрос или автоматика</param>
		public virtual void CancelAllocation(string cancellationReason, bool needUpdateOrderPaymentStatus = false, bool isByUserRequest = false)
		{
			if(IsRefundPayment || isByUserRequest)
			{
				Status = PaymentState.Cancelled;
				Comment += string.IsNullOrWhiteSpace(Comment) ? $"{cancellationReason}" : $"\n{cancellationReason}";

				if(Comment.Length > _commentLimit)
				{
					Comment = Comment.Remove(_commentLimit);
				}
			}

			foreach(var paymentItem in Items)
			{
				paymentItem.CancelAllocation(needUpdateOrderPaymentStatus);
			}
		}

		/// <summary>
		/// Валидация сущности
		/// </summary>
		/// <param name="validationContext">Контекст</param>
		/// <returns></returns>
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
			{
				yield return new ValidationResult(
					"Заполните контрагента",
					new[] { nameof(Counterparty) });
			}

			if(Comment != null && Comment.Length > _commentLimit)
			{
				yield return new ValidationResult(
					$"Длина комментария превышена на {Comment.Length - _commentLimit}",
					new[] { nameof(Comment) });
			}

			if(PaymentPurpose != null && PaymentPurpose.Length > _paymentPurposeLimit)
			{
				yield return new ValidationResult(
					$"Длина назначения платежа превышена на {PaymentPurpose.Length - _paymentPurposeLimit}",
					new[] { nameof(PaymentPurpose) });
			}

			if(Total == 0)
			{
				yield return new ValidationResult(
					$"Сумма платежа не может быть равной 0",
					new[] { nameof(Total) });
			}
		}
	}
}
