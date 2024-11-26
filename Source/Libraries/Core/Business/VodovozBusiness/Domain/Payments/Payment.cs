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
using Vodovoz.Services;
using VodovozBusiness.Domain.Payments;

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
		private bool _isManuallyCreated;
		private PaymentState _status;
		private CashlessMovementOperation _cashlessMovementOperation;
		private Counterparty _counterparty;
		private Account _counterpartyAccount;
		private ProfitCategory _profitCategory;
		private Payment _refundedPayment;
		private UserBase _currentEditorUser;
		private CashlessIncome _cashlessIncome;
		private IList<PaymentItem> _paymentItems = new List<PaymentItem>();
		GenericObservableList<PaymentItem> _observableItems;

		protected Payment() { }

		public virtual int Id { get; set; }

		/// <summary>
		/// Номер
		/// </summary>
		[Display(Name = "Номер")]
		public virtual int PaymentNum
		{
			get => _paymentNum;
			set => SetField(ref _paymentNum, value);
		}

		/// <summary>
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		/// <summary>
		/// Строки платежа
		/// </summary>
		[Display(Name = "Строки платежа")]
		public virtual IList<PaymentItem> PaymentItems
		{
			get => _paymentItems;
			set => SetField(ref _paymentItems, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaymentItem> ObservableItems
		{
			get
			{
				_observableItems = _observableItems ?? new GenericObservableList<PaymentItem>(PaymentItems);
				return _observableItems;
			}
		}

		/// <summary>
		/// Операция передвижения безнала
		/// </summary>
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

		/// <summary>
		/// Назначение платежа
		/// </summary>
		[Display(Name = "Назначение платежа")]
		public virtual string PaymentPurpose
		{
			get => _paymentPurpose;
			set => SetField(ref _paymentPurpose, value);
		}

		/// <summary>
		/// Статус платежа
		/// </summary>
		[Display(Name = "Статус платежа")]
		public virtual PaymentState Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Категория дохода
		/// </summary>
		[Display(Name = "Категория дохода")]
		public virtual ProfitCategory ProfitCategory
		{
			get => _profitCategory;
			set => SetField(ref _profitCategory, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Возвращаемый платеж
		/// </summary>
		[Display(Name = "Возвращаемый платеж")]
		public virtual Payment RefundedPayment
		{
			get => _refundedPayment;
			set => SetField(ref _refundedPayment, value);
		}

		/// <summary>
		/// "Возврат платежа по заказу №"
		/// </summary>
		[Display(Name = "Возврат платежа по заказу №")]
		public virtual int? RefundPaymentFromOrderId
		{
			get => _refundPaymentFromOrderId;
			set => SetField(ref _refundPaymentFromOrderId, value);
		}

		/// <summary>
		/// Платеж создан вручную?
		/// </summary>
		[Display(Name = "Платеж создан вручную?")]
		public virtual bool IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => SetField(ref _isManuallyCreated, value);
		}

		/// <summary>
		/// Пользователь, работающий с диалогом ручного распределения
		/// </summary>
		[Display(Name = "Пользователь, работающий с диалогом ручного распределения")]
		[IgnoreHistoryTrace]
		public virtual UserBase CurrentEditorUser
		{
			get => _currentEditorUser;
			set => SetField(ref _currentEditorUser, value);
		}
		
		/// <summary>
		/// Приход по безналу
		/// </summary>
		[Display(Name = "Приход по безналу")]
		[IgnoreHistoryTrace]
		public virtual CashlessIncome CashlessIncome
		{
			get => _cashlessIncome;
			set => SetField(ref _cashlessIncome, value);
		}

		public virtual string NumOrders { get; set; }

		public virtual bool IsRefundPayment => RefundedPayment != null;
		public virtual int OrganizationId => CashlessIncome.Organization.Id;
		public virtual string CounterpartyName { get;  }
		public virtual string CounterpartyInn { get; }
		public virtual string CounterpartyKpp { get; }
		public virtual string CounterpartyBank { get; }
		public virtual string CounterpartyAcc { get; }
		public virtual string CounterpartyCurrentAcc { get; }
		public virtual string CounterpartyCorrespondentAcc { get; }
		public virtual string CounterpartyBik { get; }
		public virtual Organization Organization { get; }
		public virtual Account OrganizationAccount { get; }

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
			var item = ObservableItems.SingleOrDefault(x =>
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

				ObservableItems.Add(paymentItem);
			}
			else
			{
				item.Sum += sum;
			}
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
			if(CashlessMovementOperation != null || IsRefundPayment)
			{
				return false;
			}

			CashlessMovementOperation = new CashlessMovementOperation
			{
				Income = Total,
				Counterparty = Counterparty,
				OrganizationId = OrganizationId,
				OperationTime = DateTime.Now,
				CashlessMovementOperationStatus = AllocationStatus.Accepted
			};

			return true;
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
				Counterparty = Counterparty,
				Status = PaymentState.undistributed,
				RefundedPayment = this,
				RefundPaymentFromOrderId = orderId
			};
		}

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

			foreach(var paymentItem in PaymentItems)
			{
				paymentItem.CancelAllocation(needUpdateOrderPaymentStatus);
			}
		}

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

		private void DefaultManuallyIncome(
			int paymentNum,
			PaymentState paymentState,
			IPaymentSettings paymentSettings,
			int? counterpartyId = null,
			DateTime? date = null)
		{
			PaymentNum = paymentNum;

			if(counterpartyId.HasValue)
			{
				Counterparty = new Counterparty
				{
					Id = counterpartyId.Value
				};
			}
			
			Date = date ?? DateTime.Today;
			
			ProfitCategory = new ProfitCategory
			{
				Id = paymentSettings.DefaultProfitCategory
			};

			IsManuallyCreated = true;
			
			Status = paymentState;
		}

		internal static Payment Create()
		{
			return new Payment();
		}

		internal static Payment CreateDefaultManuallyIncome(
			int paymentNum,
			PaymentState paymentState,
			IPaymentSettings paymentSettings,
			int? counterpartyId = null,
			DateTime? date = null)
		{
			var payment = Create();
			payment.DefaultManuallyIncome(
				paymentNum,
				paymentState,
				paymentSettings,
				counterpartyId,
				date);
			
			return payment;
		}
	}
}
