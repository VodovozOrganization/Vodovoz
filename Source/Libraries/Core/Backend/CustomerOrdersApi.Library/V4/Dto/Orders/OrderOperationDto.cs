using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	public abstract class OrderOperationDto
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
	}
}
