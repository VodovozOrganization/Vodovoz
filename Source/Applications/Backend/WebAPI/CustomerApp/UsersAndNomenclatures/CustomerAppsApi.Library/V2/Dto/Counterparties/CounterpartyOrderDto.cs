using System;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Заказ клиента
	/// </summary>
	public class CounterpartyOrderDto
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Статус заказа
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public OrderStatus OrderStatus { get; set; }
		/// <summary>
		/// Статус оплаты
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public OrderPaymentStatus PaymentStatus { get; set; }
		/// <summary>
		/// Статус заказа в текстовом виде
		/// </summary>
		public string Status { get; set; }
		/// <summary>
		/// Признак отменённого заказа
		/// </summary>
		public bool IsCanceled { get; set; }
		/// <summary>
		/// Признак доставленного заказа
		/// </summary>
		public bool IsDelivered { get; set; }
		/// <summary>
		/// Признак оплаченного заказа
		/// </summary>
		public bool IsPaid { get; set; }
	}
}
