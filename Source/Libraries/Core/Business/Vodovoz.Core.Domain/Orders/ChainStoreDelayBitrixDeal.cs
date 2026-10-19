using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Отправка сделки по отсрочке сети в Битрикс24: значения полей сделки на момент сбора, статус и результат отправки.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "отправка сделки по отсрочке сети",
		NominativePlural = "отправки сделок по отсрочке сети")]
	public class ChainStoreDelayBitrixDeal : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private DateTime? _lastUpdateDate;
		private DateTime? _lastSynchronizedDate;
		private DateTime? _closedDate;
		private ChainStoreDelayBitrixDealStatus _status;
		private long? _bitrixDealId;
		private string _lastError;
		private int _counterpartyId;
		private int _orderId;
		private string _dealTitle;
		private string _counterpartyName;
		private string _counterpartyInn;
		private string _phoneNumber;
		private string _deliveryAddress;
		private string _orderAuthor;
		private decimal _orderSum;
		private DateTime _estimatedPaymentDate;
		private int _daysOverdue;
		private decimal _debt;

		/// <summary>
		/// Идентификатор записи.
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата и время сбора (по Москве).
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Дата последнего изменения записи (ошибка, статус).
		/// </summary>
		[Display(Name = "Дата обновления")]
		public virtual DateTime? LastUpdateDate
		{
			get => _lastUpdateDate;
			set => SetField(ref _lastUpdateDate, value);
		}

		/// <summary>
		/// Дата успешного создания сделки в Битрикс24.
		/// </summary>
		[Display(Name = "Дата синхронизации")]
		public virtual DateTime? LastSynchronizedDate
		{
			get => _lastSynchronizedDate;
			set => SetField(ref _lastSynchronizedDate, value);
		}

		/// <summary>
		/// Дата закрытия сделки при оплате заказа.
		/// </summary>
		[Display(Name = "Дата закрытия сделки")]
		public virtual DateTime? ClosedDate
		{
			get => _closedDate;
			set => SetField(ref _closedDate, value);
		}

		/// <summary>
		/// Состояние отправки сделки.
		/// </summary>
		[Display(Name = "Состояние")]
		public virtual ChainStoreDelayBitrixDealStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Идентификатор сделки в Битрикс24.
		/// </summary>
		[Display(Name = "Код сделки Битрикс24")]
		public virtual long? BitrixDealId
		{
			get => _bitrixDealId;
			set => SetField(ref _bitrixDealId, value);
		}

		/// <summary>
		/// Текст последней ошибки Битрикс24.
		/// </summary>
		[Display(Name = "Последняя ошибка")]
		public virtual string LastError
		{
			get => _lastError;
			set => SetField(ref _lastError, value);
		}

		/// <summary>
		/// Идентификатор клиента в ДВ (в Битрикс24 не передаётся).
		/// </summary>
		[Display(Name = "Код клиента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Номер заказа, по которому создаётся сделка.
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Название сделки.
		/// </summary>
		[Display(Name = "Название сделки")]
		public virtual string DealTitle
		{
			get => _dealTitle;
			set => SetField(ref _dealTitle, value);
		}

		/// <summary>
		/// Наименование клиента.
		/// </summary>
		[Display(Name = "Наименование клиента")]
		public virtual string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		/// <summary>
		/// ИНН клиента.
		/// </summary>
		[Display(Name = "ИНН клиента")]
		public virtual string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		/// <summary>
		/// Телефон для связи из заказа.
		/// </summary>
		[Display(Name = "Телефон")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Адрес доставки заказа ("Самовывоз" для самовывоза).
		/// </summary>
		[Display(Name = "Адрес доставки")]
		public virtual string DeliveryAddress
		{
			get => _deliveryAddress;
			set => SetField(ref _deliveryAddress, value);
		}

		/// <summary>
		/// Автор заказа.
		/// </summary>
		[Display(Name = "Автор заказа")]
		public virtual string OrderAuthor
		{
			get => _orderAuthor;
			set => SetField(ref _orderAuthor, value);
		}

		/// <summary>
		/// Сумма заказа.
		/// </summary>
		[Display(Name = "Сумма заказа")]
		public virtual decimal OrderSum
		{
			get => _orderSum;
			set => SetField(ref _orderSum, value);
		}

		/// <summary>
		/// Расчётная дата платежа заказа.
		/// </summary>
		[Display(Name = "Расчётная дата платежа")]
		public virtual DateTime EstimatedPaymentDate
		{
			get => _estimatedPaymentDate;
			set => SetField(ref _estimatedPaymentDate, value);
		}

		/// <summary>
		/// Дни просрочки (всегда 0).
		/// </summary>
		[Display(Name = "Дней просрочки")]
		public virtual int DaysOverdue
		{
			get => _daysOverdue;
			set => SetField(ref _daysOverdue, value);
		}

		/// <summary>
		/// Долг по заказу (равен сумме заказа).
		/// </summary>
		[Display(Name = "Долг")]
		public virtual decimal Debt
		{
			get => _debt;
			set => SetField(ref _debt, value);
		}
	}
}
