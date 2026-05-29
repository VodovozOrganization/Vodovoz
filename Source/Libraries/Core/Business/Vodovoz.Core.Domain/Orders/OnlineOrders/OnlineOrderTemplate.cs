using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Шаблоны автозаказов с ИПЗ",
		Nominative = TemplateTitle,
		Prepositional = "Шаблонe автозаказа с ИПЗ",
		PrepositionalPlural = "Шаблонах автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplate : PropertyChangedBase, IDomainObject
	{
		public const string TemplateTitle = "Шаблон автозаказа с ИПЗ";
		
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
		private int _counterpartyId;
		private int _deliveryPointId;
		private int _deliveryScheduleId;
		private int _authorId;
		private int? _selfDeliveryGeoGroupId;
		private int? _callBeforeArrivalMinutes;
		private int? _bottlesReturn;
		private int? _trifle;
		private string _comment;
		private IObservableList<int> _templateProducts = new ObservableList<int>();
		private IObservableList<int> _weekdays = new ObservableList<int>();

		public OnlineOrderTemplate() { }
		
		protected OnlineOrderTemplate(
			Source source,
			int authorId,
			Guid? externalCounterpartyId,
			int counterpartyId,
			int deliveryPointId,
			int deliveryScheduleId,
			bool isSelfDelivery,
			int? selfDeliveryGeoGroupId,
			bool isFastDelivery,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			int? bottlesReturn,
			OnlineOrderDeliveryFrequency deliveryFrequency,
			OnlineOrderPaymentType paymentType,
			string contactPhone,
			string comment,
			int? trifle)
		{
			Source = source;
			AuthorId = authorId;
			ExternalCounterpartyId = externalCounterpartyId;
			CreatedAt = DateTime.Now;
			IsActive = true;
			IsArchive = false;
			IsSelfDelivery = isSelfDelivery;
			SelfDeliveryGeoGroupId = selfDeliveryGeoGroupId;
			IsFastDelivery = isFastDelivery;
			IsNeedConfirmationByCall = isNeedConfirmationByCall;
			DontArriveBeforeInterval = dontArriveBeforeInterval;
			CounterpartyId = counterpartyId;
			DeliveryPointId = deliveryPointId;
			DeliveryScheduleId = deliveryScheduleId;
			CallBeforeArrivalMinutes = callBeforeArrivalMinutes;
			BottlesReturn = bottlesReturn;
			DeliveryFrequency = deliveryFrequency;
			PaymentType = paymentType;
			ContactPhone = contactPhone;
			Comment = comment;
			Trifle = trifle;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата и время создания
		/// </summary>
		[Display(Name = "Дата и время создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}
		
		/// <summary>
		/// Источник шаблона
		/// </summary>
		[Display(Name = "Источник шаблона")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}
		
		/// <summary>
		/// Внешний Id пользователя
		/// </summary>
		[Display(Name = "Внешний Id пользователя")]
		public virtual Guid? ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}

		/// <summary>
		/// Действующий ли шаблон
		/// </summary>
		[Display(Name = "Действующий шаблон")]
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}
		
		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			protected set => SetField(ref _isArchive, value);
		}
		
		/// <summary>
		/// Самовывоз
		/// </summary>
		[Display(Name = "Самовывоз")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}
		
		/// <summary>
		/// Доставка за час
		/// </summary>
		[Display(Name = "Доставка за час")]
		public virtual bool IsFastDelivery
		{
			get => _isFastDelivery;
			set => SetField(ref _isFastDelivery, value);
		}

		/// <summary>
		/// Периодичность доставки
		/// </summary>
		[Display(Name = "Периодичность доставки")]
		public virtual OnlineOrderDeliveryFrequency DeliveryFrequency
		{
			get => _deliveryFrequency;
			set => SetField(ref _deliveryFrequency, value);
		}
		
		/// <summary>
		/// Тип оплаты
		/// </summary>
		[Display(Name = "Тип оплаты")]
		public virtual OnlineOrderPaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}
		
		/// <summary>
		/// Отзвон за
		/// </summary>
		[Display(Name = "Отзвон за")]
		public virtual int? CallBeforeArrivalMinutes
		{
			get => _callBeforeArrivalMinutes;
			set => SetField(ref _callBeforeArrivalMinutes, value);
		}

		/// <summary>
		/// Подтверждение по телефону
		/// </summary>
		[Display(Name = "Подтверждение по телефону")]
		public virtual bool IsNeedConfirmationByCall
		{
			get => _isNeedConfirmationByCall;
			set => SetField(ref _isNeedConfirmationByCall, value);
		}
		
		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		[Display(Name = "Не приезжать раньше интервала")]
		public virtual bool DontArriveBeforeInterval
		{
			get => _dontArriveBeforeInterval;
			set => SetField(ref _dontArriveBeforeInterval, value);
		}
		
		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		[Display(Name = "Бутылей на возврат")]
		public virtual int? BottlesReturn
		{
			get => _bottlesReturn;
			set => SetField(ref _bottlesReturn, value);
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
		/// Идентификатор клиента
		/// </summary>
		[Display(Name = "Идентификатор клиента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Идентификатор ТД
		/// </summary>
		[Display(Name = "Идентификатор ТД")]
		public virtual int DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}

		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		[Display(Name = "Идентификатор интервала доставки")]
		public virtual int DeliveryScheduleId
		{
			get => _deliveryScheduleId;
			set => SetField(ref _deliveryScheduleId, value);
		}
		
		/// <summary>
		/// Идентификатор автора шаблона
		/// </summary>
		[Display(Name = "Идентификатор автора шаблона")]
		public virtual int AuthorId
		{
			get => _authorId;
			set => SetField(ref _authorId, value);
		}
		
		/// <summary>
		/// Идентификатор гео группы для самовывоза
		/// </summary>
		[Display(Name = "Id гео группы для самовывоза")]
		public virtual int? SelfDeliveryGeoGroupId
		{
			get => _selfDeliveryGeoGroupId;
			set => SetField(ref _selfDeliveryGeoGroupId, value);
		}
		
		/// <summary>
		/// Номер для связи
		/// </summary>
		[Display(Name = "Номер для связи")]
		public virtual string ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}
		
		/// <summary>
		/// Сдача с
		/// </summary>
		[Display(Name = "Сдача с")]
		public virtual int? Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}
		
		/// <summary>
		/// Список идентификаторов товаров
		/// </summary>
		[Display(Name = "Список идентификаторов товаров")]
		public virtual IObservableList<int> TemplateProducts
		{
			get => _templateProducts;
			set => SetField(ref _templateProducts, value);
		}

		/// <summary>
		/// Дни недели
		/// </summary>
		[Display(Name = "Дни недели")]
		public virtual IObservableList<int> Weekdays
		{
			get => _weekdays;
			set => SetField(ref _weekdays, value);
		}

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
				return $"Новый {TemplateTitle.ToLower()}";
			}

			return $"{TemplateTitle} №{Id}";
		}

		private void Archive()
		{
			IsArchive = true;
			IsActive = false;
		}
		
		public static OnlineOrderTemplate Create(
			Source source,
			int authorId,
			Guid? externalCounterpartyId,
			int counterpartyId,
			int deliveryPointId,
			int deliveryScheduleId,
			bool isSelfDelivery,
			int? selfDeliveryGeoGroupId,
			bool isFastDelivery,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			int? bottlesReturn,
			OnlineOrderDeliveryFrequency deliveryFrequency,
			OnlineOrderPaymentType paymentType,
			string contactPhone,
			string comment,
			int? trifle
			) => new OnlineOrderTemplate(
				source,
				authorId,
				externalCounterpartyId,
				counterpartyId,
				deliveryPointId,
				deliveryScheduleId,
				isSelfDelivery,
				selfDeliveryGeoGroupId,
				isFastDelivery,
				isNeedConfirmationByCall,
				dontArriveBeforeInterval,
				callBeforeArrivalMinutes,
				bottlesReturn,
				deliveryFrequency,
				paymentType,
				contactPhone,
				comment,
				trifle);
	}
}
