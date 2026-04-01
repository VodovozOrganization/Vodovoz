using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	public class CancelOrderDto
	{
		/// <summary>
		/// Источник ИПЗ
		/// </summary>
		[Required]
		public Source Source { get; set; }

		/// <summary>
		/// Идентификатор заказа 
		/// </summary>
		[Required]
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Идентификатор транзакции
		/// </summary>
		[Required]
		public string TransactionId { get; set; }
	}
}
