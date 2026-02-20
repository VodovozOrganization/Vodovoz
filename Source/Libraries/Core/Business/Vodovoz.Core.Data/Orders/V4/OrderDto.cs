using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Orders.V4
{
	/// <summary>
	/// Информация о заказе
	/// </summary>
	public class OrderDto
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int? OrderId { get; set; }
		
		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }
		
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		
		/// <summary>
		/// Статус заказа
		/// </summary>
		public ExternalOrderStatus OrderStatus { get; set; }
		
		/// <summary>
		/// Статус оплаты онлайн заказа
		/// </summary>
		public OnlineOrderPaymentStatus? OrderPaymentStatus { get; set; }
		
		/// <summary>
		/// Дата доставки/забора заказа(самовывоз)
		/// </summary>
		public DateTime DeliveryDate { get; set; }
		
		/// <summary>
		/// Дата создания
		/// </summary>
		public DateTimeOffset CreatedDateTimeUtc { get; set; }
		
		/// <summary>
		/// Интервал доставки
		/// </summary>
		public string DeliverySchedule { get; set; }
		
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }
		
		/// <summary>
		/// Оценка заказа
		/// </summary>
		public int? RatingValue { get; set; }
		
		/// <summary>
		/// Доступна оценка заказа
		/// </summary>
		public bool IsRatingAvailable { get; set; }
		
		/// <summary>
		/// Нужна ли оплата заказа
		/// </summary>
		public bool IsNeedPay { get; set; }
		
		/// <summary>
		/// Id точки доставки
		/// </summary>
		public int? DeliveryPointId { get; set; }
		
		/// <summary>
		/// Сообщения для размещения в UI
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; set; }
	}
}
