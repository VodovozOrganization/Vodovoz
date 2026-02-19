using System;
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
		private WeekDayName _weekday;
		private RepeatOnlineOrderType _repeatOrder;
		private OnlineOrderPaymentType _paymentType;
		private int _counterpartyId;
		private int _deliveryPointId;
		private int _deliveryScheduleId;
		private IObservableList<int> _templateItems = new ObservableList<int>();

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
		/// День недели
		/// </summary>
		public virtual WeekDayName Weekday
		{
			get => _weekday;
			set => SetField(ref _weekday, value);
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
		public virtual IObservableList<int> TemplateItems
		{
			get => _templateItems;
			set => SetField(ref _templateItems, value);
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
	}
}
