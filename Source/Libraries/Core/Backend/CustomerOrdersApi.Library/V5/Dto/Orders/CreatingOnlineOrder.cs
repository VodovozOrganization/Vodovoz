using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	public class CreatingOnlineOrder : ICreatingOnlineOrder
	{
		public const string ExchangeAndQueueName = "creating-online-orders";
		/// <summary>
		/// Источник заказа
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public Source Source { get; set; }
		
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }
		
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? CounterpartyErpId { get; set; }
		
		/// <summary>
		/// Контрольная сумма заказа, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		
		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		public int? DeliveryPointId { get; set; }

		/// <summary>
		/// Самовывоз?
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		
		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		public int? SelfDeliveryGeoGroupId { get; set; }
		
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }
		
		/// <summary>
		/// Статус оплаты
		/// </summary>
		public OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }

		/// <summary>
		/// Номер оплаты
		/// </summary>
		public int? OnlinePayment { get; set; }

		/// <summary>
		/// Источник оплаты
		/// </summary>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }

		/// <summary>
		/// Нужно подтверждение по телефону?
		/// </summary>
		public bool IsNeedConfirmationByCall { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Id времени доставки из ДВ
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
		
		/// <summary>
		/// Отзвон за
		/// </summary>
		public int? CallBeforeArrivalMinutes { get; set; }
		
		/// <summary>
		/// Доставка за час?
		/// </summary>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Номер для связи
		/// </summary>
		public string ContactPhone { get; set; }

		/// <summary>
		/// Комментарий к заказу
		/// </summary>
		public string OnlineOrderComment { get; set; }

		/// <summary>
		/// Сдача с
		/// </summary>
		public int? Trifle { get; set; }

		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		public int? BottlesReturn { get; set; }
		
		/// <summary>
		/// Сумма онлайн заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		
		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		public bool DontArriveBeforeInterval { get; set; }

		/// <summary>
		/// Список товаров
		/// </summary>
		public IList<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		
		/// <summary>
		/// Список пакетов аренды
		/// </summary>
		public IList<OnlineRentPackageDto> OnlineRentPackages { get; set; }
	}
}
