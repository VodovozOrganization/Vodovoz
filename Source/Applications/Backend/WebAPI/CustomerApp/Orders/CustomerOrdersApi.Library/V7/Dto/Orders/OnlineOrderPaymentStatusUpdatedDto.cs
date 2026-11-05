using System;
using CustomerApp.Contracts.Common;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	public class OnlineOrderPaymentStatusUpdatedDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		
		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }
		
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }
		
		/// <summary>
		/// Статус оплаты
		/// </summary>
		public OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }

		/// <summary>
		/// Номер оплаты
		/// </summary>
		public int OnlinePayment { get; set; }

		/// <summary>
		/// Источник оплаты
		/// </summary>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }
	}
}
