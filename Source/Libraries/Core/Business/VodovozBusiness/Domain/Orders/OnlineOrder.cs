using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Orders
{
	public class OnlineOrder : PropertyChangedBase, IDomainObject
	{
		private Source _source;
		private int _counterpartyId;
		private Counterparty _counterparty;
		private int _deliveryPointId;
		private DeliveryPoint _deliveryPoint;
		private Guid _externalCounterpartyId;
		private bool _isSelfDelivery;
		private int? _geoGroupId;
		private OnlineOrderPaymentType _onlineOrderPaymentType;
		private OnlineOrderPaymentStatus _onlineOrderPaymentStatus;
		private int? _onlinePayment;
		private OnlinePaymentSource _onlinePaymentSource;
		private bool _isNeedConfirmationByCall;
		private DateTime _deliveryDate;
		private int _deliveryScheduleId;
		private DeliverySchedule _deliverySchedule;
		private bool _isFastDelivery;
		private string _contactPhone;
		private string _onlineOrderComment;
		private int? _trifle;
		private int? _bottlesReturn;
		private decimal _onlineOrderSum;
		private OnlineOrderStatus _onlineOrderStatus;
		private Order _order;
		private Employee _employeeWorkWith;
		private IList<OnlineOrderItem> _onlineOrderItems = new List<OnlineOrderItem>();
		private IList<OnlineRentPackage> _onlineRentPackages;

		public virtual int Id { get; set; }

		[Display(Name = "Источник онлайн заказа")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		[Display(Name = "Клиент")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
		
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Внешний Id пользователя")]
		public virtual Guid ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		[Display(Name = "Точка доставки")]
		public virtual int DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}
		
		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Самовывоз")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		[Display(Name = "Id Гео группы для самовывоза")]
		public virtual int? GeoGroupId
		{
			get => _geoGroupId;
			set => SetField(ref _geoGroupId, value);
		}

		[Display(Name = "Форма оплаты")]
		public virtual OnlineOrderPaymentType OnlineOrderPaymentType
		{
			get => _onlineOrderPaymentType;
			set => SetField(ref _onlineOrderPaymentType, value);
		}
		
		[Display(Name = "Статус оплаты")]
		public virtual OnlineOrderPaymentStatus OnlineOrderPaymentStatus
		{
			get => _onlineOrderPaymentStatus;
			set => SetField(ref _onlineOrderPaymentStatus, value);
		}
		
		[Display(Name = "Статус онлайн заказа")]
		public virtual OnlineOrderStatus OnlineOrderStatus
		{
			get => _onlineOrderStatus;
			set => SetField(ref _onlineOrderStatus, value);
		}

		[Display(Name = "Номер оплаты")]
		public virtual int? OnlinePayment
		{
			get => _onlinePayment;
			set => SetField(ref _onlinePayment, value);
		}
		
		[Display(Name = "Источник оплаты")]
		public virtual OnlinePaymentSource OnlinePaymentSource
		{
			get => _onlinePaymentSource;
			set => SetField(ref _onlinePaymentSource, value);
		}
		
		[Display(Name = "Нужно подтверждение по телефону")]
		public virtual bool IsNeedConfirmationByCall
		{
			get => _isNeedConfirmationByCall;
			set => SetField(ref _isNeedConfirmationByCall, value);
		}
		
		[Display(Name = "Дата доставки")]
		public virtual DateTime DeliveryDate
		{
			get => _deliveryDate;
			set => SetField(ref _deliveryDate, value);
		}
		
		[Display(Name = "Время доставки")]
		public virtual int DeliveryScheduleId
		{
			get => _deliveryScheduleId;
			set => SetField(ref _deliveryScheduleId, value);
		}
		
		[Display(Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}
		
		[Display(Name = "Доставка за час")]
		public virtual bool IsFastDelivery
		{
			get => _isFastDelivery;
			set => SetField(ref _isFastDelivery, value);
		}
		
		[Display(Name = "Номер для связи")]
		public virtual string ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}
		
		[Display(Name = "Комментарий к заказу")]
		public virtual string OnlineOrderComment
		{
			get => _onlineOrderComment;
			set => SetField(ref _onlineOrderComment, value);
		}
		
		[Display(Name = "Сдача с")]
		public virtual int? Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}
		
		[Display(Name = "Бутылей на возврат")]
		public virtual int? BottlesReturn
		{
			get => _bottlesReturn;
			set => SetField(ref _bottlesReturn, value);
		}
		
		[Display(Name = "Сумма онлайн заказа")]
		public virtual decimal OnlineOrderSum
		{
			get => _onlineOrderSum;
			set => SetField(ref _onlineOrderSum, value);
		}
		
		[Display(Name = "Выставленный заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
		
		[Display(Name = "У кого в работе заявка")]
		public virtual Employee EmployeeWorkWith
		{
			get => _employeeWorkWith;
			set => SetField(ref _employeeWorkWith, value);
		}
		
		[Display(Name = "Строки онлайн заказа")]
		public virtual IList<OnlineOrderItem> OnlineOrderItems
		{
			get => _onlineOrderItems;
			set => SetField(ref _onlineOrderItems, value);
		}
		
		[Display(Name = "Пакеты аренды")]
		public virtual IList<OnlineRentPackage> OnlineRentPackages
		{
			get => _onlineRentPackages;
			set => SetField(ref _onlineRentPackages, value);
		}
	}
}
