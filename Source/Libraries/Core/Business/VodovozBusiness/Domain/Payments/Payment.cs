using Gamma.Utilities;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Project.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи",
		Nominative = "платёж",
		Prepositional = "платеже",
		PrepositionalPlural = "платежах")]

	[HistoryTrace]
	[EntityPermission]
	public class Payment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _commentLimit = 300;
		private const int _paymentPurposeLimit = 300;

		private int _paymentNum;
		private int? _refundPaymentFromOrderId;
		private DateTime _date;
		private decimal _total;
		private string _paymentPurpose;
		private string _comment;
		private string _counterpartyAcc;
		private string _counterpartyCurrentAcc;
		private string _counterpartyInn;
		private string _counterpartyKpp;
		private string _counterpartyName;
		private string _counterpartyBank;
		private string _counterpartyBik;
		private string _counterpartyCorrespondentAcc;
		private bool _isManuallyCreated;
		private PaymentState _status;
		private CashlessMovementOperation _cashlessMovementOperation;
		private Counterparty _counterparty;
		private Account _counterpartyAccount;
		private Organization _organization;
		private Account _organizationAccount;
		private ProfitCategory _profitCategory;
		private Payment _refundedPayment;
		private UserBase _currentEditorUser;
		private IList<PaymentItem> _paymentItems = new List<PaymentItem>();

		public virtual int Id { get; set; }

		[Display(Name = "Номер")]
		public virtual int PaymentNum
		{
			get => _paymentNum;
			set => SetField(ref _paymentNum, value);
		}

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Сумма")]
		public virtual decimal Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		[Display(Name = "Строки платежа")]
		public virtual IList<PaymentItem> PaymentItems
		{
			get => _paymentItems;
			set => SetField(ref _paymentItems, value);
		}

		GenericObservableList<PaymentItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaymentItem> ObservableItems
		{
			get
			{
				observableItems = observableItems ?? new GenericObservableList<PaymentItem>(PaymentItems);
				return observableItems;
			}
		}

		[Display(Name = "Операция передвижения безнала")]
		public virtual CashlessMovementOperation CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public virtual Account CounterpartyAccount
		{
			get => _counterpartyAccount;
			set => SetField(ref _counterpartyAccount, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		public virtual Account OrganizationAccount
		{
			get => _organizationAccount;
			set => SetField(ref _organizationAccount, value);
		}

		[Display(Name = "Назначение платежа")]
		public virtual string PaymentPurpose
		{
			get => _paymentPurpose;
			set => SetField(ref _paymentPurpose, value);
		}

		[Display(Name = "Статус платежа")]
		public virtual PaymentState Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Категория дохода")]
		public virtual ProfitCategory ProfitCategory
		{
			get => _profitCategory;
			set => SetField(ref _profitCategory, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// р/сч плательщика
		/// </summary>
		public virtual string CounterpartyAcc
		{
			get => _counterpartyAcc;
			set => SetField(ref _counterpartyAcc, value);
		}

		/// <summary>
		/// р/сч плательщика
		/// </summary>
		public virtual string CounterpartyCurrentAcc
		{
			get => _counterpartyCurrentAcc;
			set => SetField(ref _counterpartyCurrentAcc, value);
		}

		public virtual string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		public virtual string CounterpartyKpp
		{
			get => _counterpartyKpp;
			set => SetField(ref _counterpartyKpp, value);
		}

		public virtual string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		public virtual string CounterpartyBank
		{
			get => _counterpartyBank;
			set => SetField(ref _counterpartyBank, value);
		}

		public virtual string CounterpartyBik
		{
			get => _counterpartyBik;
			set => SetField(ref _counterpartyBik, value);
		}

		public virtual string CounterpartyCorrespondentAcc
		{
			get => _counterpartyCorrespondentAcc;
			set => SetField(ref _counterpartyCorrespondentAcc, value);
		}

		[Display(Name = "Возвращаемый платеж")]
		public virtual Payment RefundedPayment
		{
			get => _refundedPayment;
			set => SetField(ref _refundedPayment, value);
		}

		[Display(Name = "Возврат платежа по заказу №")]
		public virtual int? RefundPaymentFromOrderId
		{
			get => _refundPaymentFromOrderId;
			set => SetField(ref _refundPaymentFromOrderId, value);
		}

		[Display(Name = "Платеж создан вручную?")]
		public virtual bool IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => SetField(ref _isManuallyCreated, value);
		}

		[Display(Name = "Пользователь, работающий с диалогом ручного распределения")]
		[IgnoreHistoryTrace]
		public virtual UserBase CurrentEditorUser
		{
			get => _currentEditorUser;
			set => SetField(ref _currentEditorUser, value);
		}

		public virtual string NumOrders { get; set; }

		public virtual bool IsRefundPayment => RefundedPayment != null;

		public Payment() { }

		public Payment(TransferDocument doc, Organization org, Counterparty counterparty)
		{
			PaymentNum = int.Parse(doc.DocNum);
			Date = doc.Date;
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
				Organization = org;
				OrganizationAccount = org.Accounts.FirstOrDefault(acc => acc.Number == doc.RecipientCurrentAccount);
			}

			if(counterparty != null)
			{
				Counterparty = counterparty;
				CounterpartyAccount = counterparty.Accounts.FirstOrDefault(acc => acc.Number == doc.PayerCurrentAccount);
			}
		}

		public virtual void AddPaymentItem(Order order)
		{
			var paymentItem = new PaymentItem
			{
				Order = order,
				Payment = this,
				Sum = order.OrderSum,
				PaymentItemStatus = AllocationStatus.Accepted
			};

			ObservableItems.Add(paymentItem);
		}

		public virtual void AddPaymentItem(Order order, decimal sum)
		{
			var item = ObservableItems.SingleOrDefault(x => x.Order.Id == order.Id);

			if(item == null)
			{
				var paymentItem = new PaymentItem
				{
					Order = order,
					Payment = this,
					Sum = sum,
					PaymentItemStatus = AllocationStatus.Accepted
				};

				ObservableItems.Add(paymentItem);
			}
			else
				item.Sum += sum;
		}

		public virtual void RemovePaymentItem(int paymentItemId)
		{
			var paymentItem = ObservableItems.SingleOrDefault(pi => pi.Id == paymentItemId);

			if(paymentItem != null)
			{
				ObservableItems.Remove(paymentItem);
			}
		}

		public virtual bool CreateIncomeOperation()
		{
			if(CashlessMovementOperation == null && !IsRefundPayment)
			{
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

			return false;
		}

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

		public virtual void FillPropertiesFromCounterparty()
		{
			CounterpartyInn = Counterparty.INN;
			CounterpartyKpp = Counterparty.KPP;
			CounterpartyName = Counterparty.Name;
		}

		public virtual void CancelAllocation(string cancellationReason, bool needUpdateOrderPaymentStatus = false)
		{
			if(IsRefundPayment)
			{
				Status = PaymentState.Cancelled;
				Comment += string.IsNullOrWhiteSpace(Comment) ? $"{cancellationReason}" : $"\n{cancellationReason}";

				if(Comment.Length > _commentLimit)
				{
					Comment = Comment.Remove(_commentLimit);
				}
			}

			foreach(var paymentItem in PaymentItems)
			{
				paymentItem.CancelAllocation(needUpdateOrderPaymentStatus);
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
			{
				yield return new ValidationResult("Заполните контрагента", new[] { nameof(Counterparty) });
			}

			if(Comment != null && Comment.Length > _commentLimit)
			{
				yield return new ValidationResult($"Длина комментария превышена на {Comment.Length - _commentLimit}",
					new[] { nameof(Comment) });
			}

			if(PaymentPurpose != null && PaymentPurpose.Length > _paymentPurposeLimit)
			{
				yield return new ValidationResult($"Длина назначения платежа превышена на {PaymentPurpose.Length - _paymentPurposeLimit}",
					new[] { nameof(PaymentPurpose) });
			}

			if(Total == 0)
			{
				yield return new ValidationResult($"Сумма платежа не может быть равной 0", new[] { nameof(Total) });
			}
		}
	}
}
