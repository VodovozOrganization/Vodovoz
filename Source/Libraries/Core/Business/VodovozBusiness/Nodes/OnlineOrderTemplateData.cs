using System;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Полная информация о шаблоне
	/// </summary>
	public class OnlineOrderTemplateData
	{
		private OnlineOrderTemplateData(
			int id,
			Source source,
			DateTime deliveryDate,
			Guid? externalCounterpartyId,
			bool isActive,
			bool isArchive,
			bool isSelfDelivery,
			bool isFastDelivery,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			string contactPhone,
			int? bottlesReturn,
			string comment,
			OnlineOrderDeliveryFrequency deliveryFrequency,
			OnlineOrderPaymentType paymentType,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			DeliverySchedule deliverySchedule,
			GeoGroup selfDeliveryGeoGroup,
			IObservableList<OnlineOrderTemplateProduct> templateProducts,
			IObservableList<OnlineOrderTemplateWeekday> weekdays
		)
		{
			Id = id;
			Source = source;
			DeliveryDate = deliveryDate;
			ExternalCounterpartyId = externalCounterpartyId;
			IsActive = isActive;
			IsArchive = isArchive;
			IsSelfDelivery = isSelfDelivery;
			IsFastDelivery = isFastDelivery;
			IsNeedConfirmationByCall = isNeedConfirmationByCall;
			DontArriveBeforeInterval = dontArriveBeforeInterval;
			CallBeforeArrivalMinutes = callBeforeArrivalMinutes;
			ContactPhone = contactPhone;
			BottlesReturn = bottlesReturn;
			Comment = comment;
			DeliveryFrequency = deliveryFrequency;
			PaymentType = paymentType;
			Counterparty = counterparty;
			DeliveryPoint = deliveryPoint;
			DeliverySchedule = deliverySchedule;
			SelfDeliveryGeoGroup = selfDeliveryGeoGroup;
			TemplateProducts = templateProducts;
			Weekdays = weekdays;
		}
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }
		
		public OrderTemplateDataState? State { get; set; }
		
		/// <summary>
		/// Источник шаблона
		/// </summary>
		public Source Source { get; set; }
		
		/// <summary>
		/// Внешний Id пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Действующий ли шаблон
		/// </summary>
		public bool IsActive { get; set; }
		
		/// <summary>
		/// Архивный
		/// </summary>
		public bool IsArchive { get; set; }
		
		/// <summary>
		/// Подтверждение по телефону
		/// </summary>
		public bool IsNeedConfirmationByCall { get; set; }
		
		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		public bool DontArriveBeforeInterval { get; set; }
		
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		
		/// <summary>
		/// Доставка за час
		/// </summary>
		public bool IsFastDelivery { get; set; }
		
		/// <summary>
		/// Отзвон за
		/// </summary>
		public int? CallBeforeArrivalMinutes { get; set; }
		
		/// <summary>
		/// Номер для связи
		/// </summary>
		public string ContactPhone { get; set; }
		
		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		public int? BottlesReturn { get; set; }
		
		/// <summary>
		/// Комментарий
		/// </summary>
		public string Comment { get; set; }
		
		/// <summary>
		/// Гео группы для самовывоза
		/// </summary>
		public GeoGroup SelfDeliveryGeoGroup { get; set; }

		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public OnlineOrderDeliveryFrequency DeliveryFrequency { get; set; }
		
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public OnlineOrderPaymentType PaymentType { get; set; }

		/// <summary>
		/// Клиент
		/// </summary>
		public Counterparty Counterparty { get; set; }

		/// <summary>
		/// ТД
		/// </summary>
		public DeliveryPoint DeliveryPoint { get; set; }

		/// <summary>
		/// Интервал доставки
		/// </summary>
		public DeliverySchedule DeliverySchedule { get; set; }
		
		/// <summary>
		/// Список товаров
		/// </summary>
		public IObservableList<OnlineOrderTemplateProduct> TemplateProducts { get; set; }

		/// <summary>
		/// Дни недели
		/// </summary>
		public IObservableList<OnlineOrderTemplateWeekday> Weekdays { get; set; }
		
		public static OnlineOrderTemplateData Create(
			int id,
			Source source,
			DateTime deliveryDate,
			Guid? externalCounterpartyId,
			bool isActive,
			bool isArchive,
			bool isNeedConfirmationByCall,
			bool dontArriveBeforeInterval,
			int? callBeforeArrivalMinutes,
			string contactPhone,
			int? bottlesReturn,
			string comment,
			OnlineOrderDeliveryFrequency repeatOrder,
			OnlineOrderPaymentType paymentType,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			DeliverySchedule deliverySchedule,
			IObservableList<OnlineOrderTemplateProduct> templateProducts,
			IObservableList<OnlineOrderTemplateWeekday> weekdays,
			bool isSelfDelivery = false,
			bool isFastDelivery = false,
			GeoGroup selfDeliveryGeoGroup = null
			) => new OnlineOrderTemplateData(
				id,
				source,
				deliveryDate,
				externalCounterpartyId,
				isActive,
				isArchive,
				isSelfDelivery,
				isFastDelivery,
				isNeedConfirmationByCall,
				dontArriveBeforeInterval,
				callBeforeArrivalMinutes,
				contactPhone,
				bottlesReturn,
				comment,
				repeatOrder,
				paymentType,
				counterparty,
				deliveryPoint,
				deliverySchedule,
				selfDeliveryGeoGroup,
				templateProducts,
				weekdays);
	}

	public enum OrderTemplateDataState
	{
		/// <summary>
		/// Прошел проверки
		/// </summary>
		Valid,
		/// <summary>
		/// Нужна архивация(например, промонабор в составе заархивирован)
		/// </summary>
		NeedArchive
	}
}
