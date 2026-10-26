using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Domain.Sale
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
		private OnlineOrderPaymentType _paymentType;
		private bool _isNeedConfirmationByCall;
		private bool _dontArriveBeforeInterval;
		private int _counterpartyId;
		private int _deliveryPointId;
		private int _authorId;
		private int? _selfDeliveryGeoGroupId;
		private int? _callBeforeArrivalMinutes;
		private int? _bottlesReturn;
		private int? _trifle;
		private string _comment;
		private IObservableList<int> _templateProducts = new ObservableList<int>();
		private TemplateDeliveryScheduleData _schedule;
		private OnlineOrderTemplateStatus _status;
		private OnlineRecurrentPayment _recurrentPayment;

		public OnlineOrderTemplate() { }
		
		protected OnlineOrderTemplate(
			Source source,
			int authorId,
			Guid? externalCounterpartyId,
			int counterpartyId,
			int deliveryPointId,
			bool isSelfDelivery,
			int? selfDeliveryGeoGroupId,
			bool isFastDelivery,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			int? bottlesReturn,
			OnlineOrderPaymentType paymentType,
			string contactPhone,
			string comment,
			TemplateDeliveryScheduleData schedule,
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
			CallBeforeArrivalMinutes = callBeforeArrivalMinutes;
			BottlesReturn = bottlesReturn;
			PaymentType = paymentType;
			ContactPhone = contactPhone;
			Comment = comment;
			Schedule = schedule;
			Trifle = trifle;
			
			UpdateStateByPaymentType();
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
		/// Тип оплаты
		/// </summary>
		[Display(Name = "Тип оплаты")]
		public virtual OnlineOrderPaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		/// <summary>
		/// Данные по доставке автозаказа
		/// </summary>
		[Display(Name = "Данные по доставке автозаказа")]
		public virtual TemplateDeliveryScheduleData Schedule
		{
			get => _schedule;
			set => SetField(ref _schedule, value);
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
		/// Рекуррентный платеж
		/// </summary>
		[Display(Name = "Рекуррентный платеж")]
		public virtual OnlineRecurrentPayment RecurrentPayment
		{
			get => _recurrentPayment;
			set => SetField(ref _recurrentPayment, value);
		}

		/// <summary>
		/// Обновление состояния шаблона
		/// </summary>
		public virtual void UpdateStateByPaymentType()
		{
			Status = PostPayments.Contains(PaymentType)
				? OnlineOrderTemplateStatus.Active
				: OnlineOrderTemplateStatus.AwaitingPayment;
		}

		public virtual OnlineOrderTemplateStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public override string ToString()
		{
			if(Id == 0)
			{
				return $"Новый {TemplateTitle.ToLower()}";
			}

			return $"{TemplateTitle} №{Id}";
		}

		public virtual void Archive()
		{
			Status = OnlineOrderTemplateStatus.Archived;
		}

		public static OnlineOrderPaymentType[] PostPayments => new OnlineOrderPaymentType[]
		{
			OnlineOrderPaymentType.Cash,
			OnlineOrderPaymentType.Terminal
		};
		
		public static OnlineOrderTemplate Create(
			Source source,
			int authorId,
			Guid? externalCounterpartyId,
			int counterpartyId,
			int deliveryPointId,
			bool isSelfDelivery,
			int? selfDeliveryGeoGroupId,
			bool isFastDelivery,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			int? bottlesReturn,
			OnlineOrderPaymentType paymentType,
			string contactPhone,
			string comment,
			TemplateDeliveryScheduleData schedule,
			int? trifle
			) => new OnlineOrderTemplate(
				source,
				authorId,
				externalCounterpartyId,
				counterpartyId,
				deliveryPointId,
				isSelfDelivery,
				selfDeliveryGeoGroupId,
				isFastDelivery,
				isNeedConfirmationByCall,
				dontArriveBeforeInterval,
				callBeforeArrivalMinutes,
				bottlesReturn,
				paymentType,
				contactPhone,
				comment,
				schedule,
				trifle);

		public static OnlineRecurrentPayment CreateRecurrentPayment(
			Source source,
			int templateId,
			Guid? externalCounterpartyId,
			TemplateRecurrentPaymentData recurrentData)
			=> OnlineRecurrentPayment.Create(source, templateId, externalCounterpartyId, recurrentData);
	}

	public enum TemplateDeliveryScheduleType
	{
		Weekly,
		Monthly
	}

	public class TemplateDeliveryScheduleData : PropertyChangedBase
	{
		private TemplateDeliveryScheduleType _deliveryScheduleType;
		private WeekDayName? _weekday;
		private int? _repeatEveryWeeks;
		private DateTime? _firstDeliveryDate;
		private int? _dayOfMonth;
		private int _deliveryScheduleId;
		
		protected TemplateDeliveryScheduleData() { }

		protected TemplateDeliveryScheduleData(
			TemplateDeliveryScheduleType scheduleType,
			int deliveryScheduleId,
			WeekDayName? weekday,
			int? repeatEveryWeeks,
			DateTime? firstDeliveryDate,
			int? dayOfMonth
			)
		{
			ScheduleType = scheduleType;
			DeliveryScheduleId = deliveryScheduleId;
			Weekday = weekday;
			RepeatEveryWeeks = repeatEveryWeeks;
			FirstDeliveryDate = firstDeliveryDate;
			DayOfMonth = dayOfMonth;
			
			CheckInitializedProperties();
		}

		private void CheckInitializedProperties()
		{
			if(ScheduleType == TemplateDeliveryScheduleType.Weekly)
			{
				if(!_weekday.HasValue)
				{
					throw new ArgumentNullException(
						nameof(Weekday),
						"Для недельной периодичности обязательно должен быть заполнен день недели!");
				}
				
				if(!_repeatEveryWeeks.HasValue)
				{
					throw new ArgumentNullException(
						nameof(RepeatEveryWeeks),
						"Для недельной периодичности обязательно должна быть заполнена периодичность!");
				}
				
				if(!_firstDeliveryDate.HasValue)
				{
					throw new ArgumentNullException(
						nameof(FirstDeliveryDate),
						"Для недельной периодичности обязательно должна быть заполнена дата первой поставки!");
				}
			}
			else
			{
				if(!_dayOfMonth.HasValue)
				{
					throw new ArgumentNullException(
						nameof(DayOfMonth),
						"Для месячной периодичности обязательно должно быть заполнено число месяца!");
				}
			}
		}

		public virtual TemplateDeliveryScheduleType ScheduleType
		{
			get => _deliveryScheduleType;
			protected set => SetField(ref _deliveryScheduleType, value);
		}
		
		/// <summary>
		/// День недели
		/// </summary>
		[Display(Name = "День недели")]
		public virtual WeekDayName? Weekday
		{
			get => _weekday;
			protected set => SetField(ref _weekday, value);
		}

		/// <summary>
		/// Повторы через определенное количество недель
		/// Доступно только при недельном типе доставки <see cref="TemplateDeliveryScheduleType.Weekly"/> 
		/// 1 - каждую неделю
		/// 2 - раз в две недели
		/// 3 - раз в три недели и т.д.
		/// </summary>
		public virtual int? RepeatEveryWeeks
		{
			get => _repeatEveryWeeks;
			protected set => SetField(ref _repeatEveryWeeks, value);
		}

		/// <summary>
		/// Дата первой доставки по автозаказу
		/// </summary>
		public virtual DateTime? FirstDeliveryDate
		{
			get => _firstDeliveryDate;
			protected set => SetField(ref _firstDeliveryDate, value);
		}

		/// <summary>
		/// Число месяца
		/// Доступно только при месячном типе доставки <see cref="TemplateDeliveryScheduleType.Monthly"/>
		/// </summary>
		public virtual int? DayOfMonth
		{
			get => _dayOfMonth;
			protected set => SetField(ref _dayOfMonth, value);
		}

		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		public virtual int DeliveryScheduleId
		{
			get => _deliveryScheduleId;
			set => SetField(ref _deliveryScheduleId, value);
		}

		public static TemplateDeliveryScheduleData Create(
			TemplateDeliveryScheduleType scheduleType,
			int deliveryScheduleId,
			WeekDayName? weekday,
			int? repeatEveryWeeks,
			DateTime? firstDeliveryDate,
			int? dayOfMonth) =>
			new TemplateDeliveryScheduleData(scheduleType, deliveryScheduleId, weekday, repeatEveryWeeks, firstDeliveryDate, dayOfMonth);
	}
}
