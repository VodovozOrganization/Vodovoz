using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Данные синхронизации недовоза со сделкой в Битрикс24.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "синхронизация недовоза с Битрикс24",
		NominativePlural = "синхронизации недовозов с Битрикс24")]
	public class UndeliveredOrderBitrixDeal : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private DateTime _lastUpdateDate;
		private DateTime? _lastSynchronizedDate;
		private BitrixDealSynchronizationStatus _status;
		private int _undeliveredOrderId;
		private long? _bitrixDealId;
		private DateTime _sourceLastEditedTime;
		private string _dealTitle;
		private string _orderId;
		private string _counterpartyName;
		private string _deliveryAddress;
		private DateTime? _deliveryDate;
		private string _deliveryInterval;
		private string _driverName;
		private string _routeListId;
		private string _typeOfUndelivery;
		private string _specificationOfUndelivery;
		private string _comment;
		private string _newOrderId;
		private string _lastError;

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
		/// Дата создания записи синхронизации.
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Дата последнего обновления записи синхронизации.
		/// </summary>
		[Display(Name = "Дата обновления")]
		public virtual DateTime LastUpdateDate
		{
			get => _lastUpdateDate;
			set => SetField(ref _lastUpdateDate, value);
		}

		/// <summary>
		/// Дата последней успешной синхронизации с Битрикс24.
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
		/// Идентификатор недовоза в ДВ.
		/// </summary>
		[Display(Name = "Код недовоза")]
		public virtual int UndeliveredOrderId
		{
			get => _undeliveredOrderId;
			set => SetField(ref _undeliveredOrderId, value);
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
		/// Время последнего изменения исходного недовоза.
		/// </summary>
		[Display(Name = "Время изменения недовоза")]
		public virtual DateTime SourceLastEditedTime
		{
			get => _sourceLastEditedTime;
			set => SetField(ref _sourceLastEditedTime, value);
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
		/// Номер заказа, по которому создан недовоз.
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual string OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
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
		/// Адрес доставки.
		/// </summary>
		[Display(Name = "Адрес доставки")]
		public virtual string DeliveryAddress
		{
			get => _deliveryAddress;
			set => SetField(ref _deliveryAddress, value);
		}

		/// <summary>
		/// Дата заказа.
		/// </summary>
		[Display(Name = "Дата заказа")]
		public virtual DateTime? DeliveryDate
		{
			get => _deliveryDate;
			set => SetField(ref _deliveryDate, value);
		}

		/// <summary>
		/// Интервал доставки.
		/// </summary>
		[Display(Name = "Интервал доставки")]
		public virtual string DeliveryInterval
		{
			get => _deliveryInterval;
			set => SetField(ref _deliveryInterval, value);
		}

		/// <summary>
		/// ФИО водителя.
		/// </summary>
		[Display(Name = "Водитель")]
		public virtual string DriverName
		{
			get => _driverName;
			set => SetField(ref _driverName, value);
		}

		/// <summary>
		/// Номер маршрутного листа.
		/// </summary>
		[Display(Name = "Маршрутный лист")]
		public virtual string RouteListId
		{
			get => _routeListId;
			set => SetField(ref _routeListId, value);
		}

		/// <summary>
		/// Вид недовоза.
		/// </summary>
		[Display(Name = "Вид недовоза")]
		public virtual string TypeOfUndelivery
		{
			get => _typeOfUndelivery;
			set => SetField(ref _typeOfUndelivery, value);
		}

		/// <summary>
		/// Детализация недовоза.
		/// </summary>
		[Display(Name = "Детализация недовоза")]
		public virtual string SpecificationOfUndelivery
		{
			get => _specificationOfUndelivery;
			set => SetField(ref _specificationOfUndelivery, value);
		}

		/// <summary>
		/// Комментарий из поля "Что случилось?".
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Номер нового заказа.
		/// </summary>
		[Display(Name = "Новый заказ")]
		public virtual string NewOrderId
		{
			get => _newOrderId;
			set => SetField(ref _newOrderId, value);
		}

		/// <summary>
		/// Последняя ошибка синхронизации.
		/// </summary>
		[Display(Name = "Последняя ошибка")]
		public virtual string LastError
		{
			get => _lastError;
			set => SetField(ref _lastError, value);
		}
	}
}
