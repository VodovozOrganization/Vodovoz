using System;

namespace CustomerOrders.Contracts.V5.Orders
{
	/// <summary>
	/// Данные для изменения заказа
	/// </summary>
	public class ChangingOrderDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }
		/// <summary>
		/// Номер онлайн оплаты
		/// </summary>
		public int? OnlinePayment { get; set; }
		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id пользователя в ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public ExternalOrderPaymentType? OnlineOrderPaymentType { get; set; }
		/// <summary>
		/// Источник оплаты
		/// </summary>
		public ExternalPaymentSource? OnlinePaymentSource { get; set; }
		/// <summary>
		/// Статус оплаты онлайн заказа
		/// </summary>
		public ExternalOrderPaymentStatus? PaymentStatus { get; set; }
		/// <summary>
		/// Причина, по которой не прошла оплата
		/// </summary>
		public string UnPaidReason { get; set; }
		/// <summary>
		/// Дата доставки/забора заказа(самовывоз)
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Интервал доставки
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }
	}
}
