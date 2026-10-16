using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
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
		private BitrixDealSynchronizationStatus _status;
		private long? _bitrixDealId;
		private string _lastError;
		private int _counterpartyId;
		private int _initiatorOrderId;
		private string _dealTitle;
		private string _counterpartyName;
		private string _counterpartyInn;
		private string _phoneNumber;
		private string _deliveryAddresses;
		private string _orderAuthors;
		private decimal _ordersSum;
		private DateTime _initiatorEstimatedPaymentDate;
		private int _daysOverdue;
		private decimal _debtSum;
		private IObservableList<ChainStoreDelayBitrixDealOrder> _orders = new ObservableList<ChainStoreDelayBitrixDealOrder>();

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
		/// Статус синхронизации сделки.
		/// </summary>
		[Display(Name = "Статус")]
		public virtual BitrixDealSynchronizationStatus Status
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
		/// Номер заказа-инициатора.
		/// </summary>
		[Display(Name = "Заказ-инициатор")]
		public virtual int InitiatorOrderId
		{
			get => _initiatorOrderId;
			set => SetField(ref _initiatorOrderId, value);
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
		/// Телефон для связи из заказа-инициатора.
		/// </summary>
		[Display(Name = "Телефон")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Адреса доставки группы заказов через запятую.
		/// </summary>
		[Display(Name = "Адреса доставки")]
		public virtual string DeliveryAddresses
		{
			get => _deliveryAddresses;
			set => SetField(ref _deliveryAddresses, value);
		}

		/// <summary>
		/// Авторы заказов группы через запятую.
		/// </summary>
		[Display(Name = "Авторы заказов")]
		public virtual string OrderAuthors
		{
			get => _orderAuthors;
			set => SetField(ref _orderAuthors, value);
		}

		/// <summary>
		/// Сумма заказов группы.
		/// </summary>
		[Display(Name = "Сумма заказов")]
		public virtual decimal OrdersSum
		{
			get => _ordersSum;
			set => SetField(ref _ordersSum, value);
		}

		/// <summary>
		/// Расчётная дата платежа заказа-инициатора.
		/// </summary>
		[Display(Name = "Расчётная дата платежа инициатора")]
		public virtual DateTime InitiatorEstimatedPaymentDate
		{
			get => _initiatorEstimatedPaymentDate;
			set => SetField(ref _initiatorEstimatedPaymentDate, value);
		}

		/// <summary>
		/// Дни просрочки самого раннего заказа группы.
		/// </summary>
		[Display(Name = "Дней просрочки")]
		public virtual int DaysOverdue
		{
			get => _daysOverdue;
			set => SetField(ref _daysOverdue, value);
		}

		/// <summary>
		/// Долг по группе заказов.
		/// </summary>
		[Display(Name = "Сумма долга")]
		public virtual decimal DebtSum
		{
			get => _debtSum;
			set => SetField(ref _debtSum, value);
		}

		/// <summary>
		/// Состав группы заказов.
		/// </summary>
		[Display(Name = "Заказы")]
		public virtual IObservableList<ChainStoreDelayBitrixDealOrder> Orders
		{
			get => _orders;
			set => SetField(ref _orders, value);
		}
	}
}
