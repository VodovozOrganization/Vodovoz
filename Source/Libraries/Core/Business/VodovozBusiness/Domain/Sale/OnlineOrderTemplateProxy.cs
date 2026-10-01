using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public class OnlineOrderTemplateProxy : PropertyChangedBase, ISaleSource
	{
		private DateTime _createdAt;
		private Source _source;
		private Guid? _externalCounterpartyId;
		private bool _isActive;
		private bool _isArchive;
		private bool _isSelfDelivery;
		private bool _isFastDelivery;
		private string _contactPhone;
		private OnlineOrderDeliveryFrequency _deliveryFrequency;
		private OnlineOrderPaymentType _paymentType;
		private bool _isNeedConfirmationByCall;
		private bool _dontArriveBeforeInterval;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private DeliverySchedule _deliverySchedule;
		private Employee _author;
		private GeoGroup _selfDeliveryGeoGroup;
		private int? _callBeforeArrivalMinutes;
		private int? _bottlesReturn;
		private int? _trifle;
		private string _comment;
		private ObservableList<OnlineOrderTemplateSaleItem> _saleItems = new ObservableList<OnlineOrderTemplateSaleItem>();
		private IObservableList<OnlineOrderTemplateWeekday> _weekdays = new ObservableList<OnlineOrderTemplateWeekday>();
		private PaymentType? _paymentType1;

		public OnlineOrderTemplateProxy() { }

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата и время создания
		/// </summary>
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}
		
		/// <summary>
		/// Источник шаблона
		/// </summary>
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}
		
		/// <summary>
		/// Внешний Id пользователя
		/// </summary>
		public virtual Guid? ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}

		/// <summary>
		/// Действующий ли шаблон
		/// </summary>
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}
		
		/// <summary>
		/// Архивный
		/// </summary>
		public virtual bool IsArchive
		{
			get => _isArchive;
			protected set => SetField(ref _isArchive, value);
		}
		
		/// <summary>
		/// Самовывоз
		/// </summary>
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}
		
		/// <summary>
		/// Доставка за час
		/// </summary>
		public virtual bool IsFastDelivery
		{
			get => _isFastDelivery;
			set => SetField(ref _isFastDelivery, value);
		}

		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public virtual OnlineOrderDeliveryFrequency DeliveryFrequency
		{
			get => _deliveryFrequency;
			set => SetField(ref _deliveryFrequency, value);
		}
		
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public virtual OnlineOrderPaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		/// <summary>
		/// Отзвон за
		/// </summary>
		public virtual int? CallBeforeArrivalMinutes
		{
			get => _callBeforeArrivalMinutes;
			set => SetField(ref _callBeforeArrivalMinutes, value);
		}

		/// <summary>
		/// Подтверждение по телефону
		/// </summary>
		public virtual bool IsNeedConfirmationByCall
		{
			get => _isNeedConfirmationByCall;
			set => SetField(ref _isNeedConfirmationByCall, value);
		}
		
		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		public virtual bool DontArriveBeforeInterval
		{
			get => _dontArriveBeforeInterval;
			set => SetField(ref _dontArriveBeforeInterval, value);
		}
		
		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		public virtual int? BottlesReturn
		{
			get => _bottlesReturn;
			set => SetField(ref _bottlesReturn, value);
		}
		
		/// <summary>
		/// Комментарий
		/// </summary>
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Клиент
		/// </summary>
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Точка доставки
		/// </summary>
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		/// <summary>
		/// Интервал доставки
		/// </summary>
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}
		
		/// <summary>
		/// Автор шаблона
		/// </summary>
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}
		
		/// <summary>
		/// Гео группа самовывоза
		/// </summary>
		public virtual GeoGroup SelfDeliveryGeoGroup
		{
			get => _selfDeliveryGeoGroup;
			set => SetField(ref _selfDeliveryGeoGroup, value);
		}
		
		/// <summary>
		/// Номер для связи
		/// </summary>
		public virtual string ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}
		
		/// <summary>
		/// Сдача с
		/// </summary>
		public virtual int? Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}
		
		/// <summary>
		/// Позиции на продажу
		/// </summary>
		public virtual ObservableList<OnlineOrderTemplateSaleItem> SaleItems
		{
			get => _saleItems;
			set => SetField(ref _saleItems, value);
		}

		/// <summary>
		/// Дни недели
		/// </summary>
		public virtual IObservableList<OnlineOrderTemplateWeekday> Weekdays
		{
			get => _weekdays;
			set => SetField(ref _weekdays, value);
		}

		#region ISaleSource implementation

		PaymentType? ISaleSource.PaymentType => PaymentType.ToOrderPaymentType();
		public virtual DateTime? DeliveryDate => null;
		public virtual bool IsLoadedFrom1C => false;
		public virtual bool HasDeposits => false;
		public virtual bool HasNonPaidDeliveries => false;
		public virtual IList SaleItemsList => SaleItems;
		public virtual bool HasPermissionsForAlternativePrice => false;
		IEnumerable<ISaleItem> ISaleItems.SaleItems => SaleItems;

		#endregion

		/// <summary>
		/// Обновление состояния шаблона
		/// </summary>
		/// <param name="isActive">Признак активности</param>
		/// <param name="isArchive">Признак архивности</param>
		public virtual void UpdateState(bool isActive, bool isArchive)
		{
			if(isArchive)
			{
				Archive();
			}
			else if(IsArchive)
			{
				// возможно стоит ответить, что нельзя разархивировать
			}
			else
			{
				IsActive = isActive;
			}
		}
		
		public virtual OnlineOrderTemplateStatus Status => IsActive ? OnlineOrderTemplateStatus.Active : OnlineOrderTemplateStatus.Inactive;

		public override string ToString()
		{
			if(Id == 0)
			{
				return $"Новый {OnlineOrderTemplate.TemplateTitle.ToLower()}";
			}

			return $"{OnlineOrderTemplate.TemplateTitle} №{Id}";
		}

		private void Archive()
		{
			IsArchive = true;
			IsActive = false;
		}

	}
}
