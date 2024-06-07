﻿using System;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Contracts
{
	/// <summary>
	/// Данные, отправляемые в ИПЗ при смене статуса онлайн заказа в ДВ
	/// </summary>
	public class OnlineOrderStatusUpdatedDto
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
		public ExternalOrderStatus OrderStatus { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Id времени доставки из ДВ
		/// </summary>
		public int? DeliveryScheduleId { get; set; }
	}
}
