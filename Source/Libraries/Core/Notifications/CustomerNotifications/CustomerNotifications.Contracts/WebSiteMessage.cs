using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Orders;

namespace CustomerNotifications.Contracts
{
	/// <summary>
	/// Сообщение для отправки уведомления клиенту о статусе заказа для сайта
	/// </summary>
	public class WebSiteMessage
	{
		/// <summary>
		/// Номер заказа в ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }
		/// <summary>
		/// Номер онлайн заказа в ДВ
		/// </summary>
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Номер заказа в ДВ
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Статус заказа для ИПЗ
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ExternalOrderStatus OrderStatus { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Id времени доставки из ДВ
		/// </summary>
		public int? DeliveryScheduleId { get; set; }

		/// <summary>
		/// Текст уведомления
		/// </summary>
		public string PushText { get; set; }
	}
}
