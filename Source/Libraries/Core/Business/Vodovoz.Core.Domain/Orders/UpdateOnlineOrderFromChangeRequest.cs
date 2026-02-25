using System;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Данные для обновления онлайн заказа
	/// </summary>
	public class UpdateOnlineOrderFromChangeRequest
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
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
		public int CounterpartyErpId { get; set; }
		/// <summary>
		/// Id пользователя в ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType? OnlineOrderPaymentType { get; set; }
		/// <summary>
		/// Источник оплаты
		/// </summary>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }
		/// <summary>
		/// Статус оплаты онлайн заказа
		/// </summary>
		public OnlineOrderPaymentStatus? PaymentStatus { get; set; }
		/// <summary>
		/// Причина, по которой не прошла оплата
		/// </summary>
		public string UnPaidReason { get; set; }
		/// <summary>
		/// Дата доставки/забора заказа(самовывоз)
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Id интервала доставки
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }
	}
}
