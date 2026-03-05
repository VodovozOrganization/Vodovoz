using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Шаблоны автозаказов с ИПЗ",
		Nominative = "Шаблон автозаказа с ИПЗ",
		Prepositional = "Шаблонe автозаказа с ИПЗ",
		PrepositionalPlural = "Шаблонах автозаказов с ИПЗ"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplate : PropertyChangedBase, IDomainObject
	{
		private DateTime _createdAt;
		private bool _isActive;
		private bool _isArchive;
		private RepeatOnlineOrderType _repeatOrder;
		private OnlineOrderPaymentType _paymentType;
		private int _counterpartyId;
		private int _deliveryPointId;
		private int _deliveryScheduleId;
		private IObservableList<int> _templateProducts = new ObservableList<int>();
		private IObservableList<WeekDayName> _weekdays = new ObservableList<WeekDayName>();

		protected OnlineOrderTemplate(
			int counterpartyId,
			int deliveryPointId,
			int deliveryScheduleId,
			RepeatOnlineOrderType repeatOrder,
			OnlineOrderPaymentType paymentType,
			IEnumerable<WeekDayName> weekdays)
		{
			CreatedAt = DateTime.Now;
			IsActive = true;
			IsArchive = false;
			CounterpartyId = counterpartyId;
			DeliveryPointId = deliveryPointId;
			DeliveryScheduleId = deliveryScheduleId;
			RepeatOrder = repeatOrder;
			PaymentType = paymentType;
			Weekdays = new ObservableList<WeekDayName>(weekdays);
		}

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
		/// Интервал повторов
		/// </summary>
		public virtual RepeatOnlineOrderType RepeatOrder
		{
			get => _repeatOrder;
			set => SetField(ref _repeatOrder, value);
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
		/// Идентификатор клиента
		/// </summary>
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Идентификатор ТД
		/// </summary>
		public virtual int DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}

		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		public virtual int DeliveryScheduleId
		{
			get => _deliveryScheduleId;
			set => SetField(ref _deliveryScheduleId, value);
		}
		
		/// <summary>
		/// Список идентификаторов товаров
		/// </summary>
		public virtual IObservableList<int> TemplateProducts
		{
			get => _templateProducts;
			set => SetField(ref _templateProducts, value);
		}

		/// <summary>
		/// Дни недели
		/// </summary>
		public virtual IObservableList<WeekDayName> Weekdays
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
			else
			{
				IsActive = isActive;
				IsArchive = isArchive;
			}
		}
		
		private void Archive()
		{
			IsArchive = true;
			IsActive = false;
		}
		
		public static OnlineOrderTemplate Create(
			int counterpartyId,
			int deliveryPointId,
			int deliveryScheduleId,
			RepeatOnlineOrderType repeatOrder,
			OnlineOrderPaymentType paymentType,
			IEnumerable<WeekDayName> weekdays
			) => new OnlineOrderTemplate(counterpartyId, deliveryPointId, deliveryScheduleId, repeatOrder, paymentType, weekdays);
	}
}
