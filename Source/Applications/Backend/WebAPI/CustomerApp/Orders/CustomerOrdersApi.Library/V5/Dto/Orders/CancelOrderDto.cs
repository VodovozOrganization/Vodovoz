using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	public class CancelOrderDto
	{
		/// <summary>
		/// Источник ИПЗ
		/// </summary>
		[Required]
		public Source Source { get; set; }

		/// <summary>
		/// Идентификатор клиента ДВ 
		/// </summary>
		[Required]
		public int ErpCounterpartyId { get; set; }

		/// <summary>
		/// ID заказа (опционально)
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// ID онлайн-заказа (опционально)
		/// </summary>
		public int? OnlineOrderId { get; set; }
	}
}
