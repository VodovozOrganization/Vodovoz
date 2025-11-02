using System;
using System.ComponentModel.DataAnnotations;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Платеж
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи(сокр)",
		Nominative = "платёж(сокр)",
		Prepositional = "платеже(сокр)",
		PrepositionalPlural = "платежах(сокр)")]
	public class PaymentEntity : PropertyChangedBase, IDomainObject
	{
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
		private CashlessMovementOperationEntity _cashlessMovementOperation;
		private CounterpartyEntity _counterparty;
		private Account _counterpartyAccount;
		private OrganizationEntity _organization;
		private Account _organizationAccount;
		private ProfitCategory _profitCategory;
		private PaymentEntity _refundedPayment;
		private IObservableList<PaymentItemEntity> _items = new ObservableList<PaymentItemEntity>();

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
		public virtual IObservableList<PaymentItemEntity> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Операция передвижения безнала
		/// </summary>
		[Display(Name = "Операция передвижения безнала")]
		public virtual CashlessMovementOperationEntity CashlessMovementOperation
		{
			get => _cashlessMovementOperation;
			set => SetField(ref _cashlessMovementOperation, value);
		}

		/// <summary>
		/// Клиент
		/// </summary>
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Р/сч клиента
		/// </summary>
		public virtual Account CounterpartyAccount
		{
			get => _counterpartyAccount;
			set => SetField(ref _counterpartyAccount, value);
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Р/сч организации
		/// </summary>
		public virtual Account OrganizationAccount
		{
			get => _organizationAccount;
			set => SetField(ref _organizationAccount, value);
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

		/// <summary>
		/// ИНН плательщика
		/// </summary>
		public virtual string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		/// <summary>
		/// КПП плательщика
		/// </summary>
		public virtual string CounterpartyKpp
		{
			get => _counterpartyKpp;
			set => SetField(ref _counterpartyKpp, value);
		}

		/// <summary>
		/// Наименование плательщика
		/// </summary>
		public virtual string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		/// <summary>
		/// Банк плательщика
		/// </summary>
		public virtual string CounterpartyBank
		{
			get => _counterpartyBank;
			set => SetField(ref _counterpartyBank, value);
		}

		/// <summary>
		/// БИК плательщика
		/// </summary>
		public virtual string CounterpartyBik
		{
			get => _counterpartyBik;
			set => SetField(ref _counterpartyBik, value);
		}

		/// <summary>
		/// Кор счет плательщика
		/// </summary>
		public virtual string CounterpartyCorrespondentAcc
		{
			get => _counterpartyCorrespondentAcc;
			set => SetField(ref _counterpartyCorrespondentAcc, value);
		}

		/// <summary>
		/// Возвращаемый платеж
		/// </summary>
		[Display(Name = "Возвращаемый платеж")]
		public virtual PaymentEntity RefundedPayment
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

		public virtual string NumOrders { get; set; }

		public virtual PaymentEntity GetFirstParentRefundedPaymentOrCurrent()
		{
			if(RefundedPayment is null)
			{
				return this;
			}

			var payment = RefundedPayment;
			while(payment.RefundedPayment != null)
			{
				payment = payment.RefundedPayment;
			}
			
			return payment;
		}
	}
}
