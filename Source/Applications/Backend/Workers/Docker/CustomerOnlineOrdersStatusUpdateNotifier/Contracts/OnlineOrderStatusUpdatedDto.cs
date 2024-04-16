﻿using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Data.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Contracts
{
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
	}
}
