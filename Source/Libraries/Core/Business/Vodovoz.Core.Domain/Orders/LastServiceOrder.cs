using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Последний сервисный заказ
	/// Данные о последнем выполненном заказе клиента с номенклатурами санитарной обработки,
	/// после которого клиент долго не делал новых сервисных заказов,
	/// для создания сделки в Битрикс24
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "последние сервисные заказы",
		Nominative = "последний сервисный заказ"
	)]
	public class LastServiceOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private BitrixDealCreationStage _stage;
		private int _counterpartyId;
		private string _counterpartyName;
		private int? _deliveryPointId;
		private bool _isSelfDelivery;
		private string _deliveryPointAddress;
		private string _phoneNumber;
		private string _emailAddress;
		private int _lastOrderId;
		private DateTime _lastOrderDeliveryDate;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата и время сбора данных
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Стадия обработки последнего сервисного заказа
		/// </summary>
		[Display(Name = "Стадия")]
		public virtual BitrixDealCreationStage Stage
		{
			get => _stage;
			set => SetField(ref _stage, value);
		}

		/// <summary>
		/// Id контрагента
		/// </summary>
		[Display(Name = "Код контрагента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		[Display(Name = "Наименование контрагента")]
		public virtual string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		/// <summary>
		/// Id точки доставки последнего сервисного заказа, null - для самовывоза
		/// </summary>
		[Display(Name = "Код точки доставки")]
		public virtual int? DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}

		/// <summary>
		/// Признак самовывоза последнего сервисного заказа
		/// </summary>
		[Display(Name = "Самовывоз")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		/// <summary>
		/// Адрес точки доставки последнего сервисного заказа, пусто - для самовывоза
		/// </summary>
		[Display(Name = "Адрес точки доставки")]
		public virtual string DeliveryPointAddress
		{
			get => _deliveryPointAddress;
			set => SetField(ref _deliveryPointAddress, value);
		}

		/// <summary>
		/// Номер для связи, указанный в последнем сервисном заказе
		/// </summary>
		[Display(Name = "Номер телефона")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[Display(Name = "Адрес электронной почты")]
		public virtual string EmailAddress
		{
			get => _emailAddress;
			set => SetField(ref _emailAddress, value);
		}

		/// <summary>
		/// Id последнего выполненного сервисного заказа
		/// </summary>
		[Display(Name = "Код последнего сервисного заказа")]
		public virtual int LastOrderId
		{
			get => _lastOrderId;
			set => SetField(ref _lastOrderId, value);
		}

		/// <summary>
		/// Дата доставки последнего выполненного сервисного заказа
		/// </summary>
		[Display(Name = "Дата доставки последнего сервисного заказа")]
		public virtual DateTime LastOrderDeliveryDate
		{
			get => _lastOrderDeliveryDate;
			set => SetField(ref _lastOrderDeliveryDate, value);
		}
	}
}
