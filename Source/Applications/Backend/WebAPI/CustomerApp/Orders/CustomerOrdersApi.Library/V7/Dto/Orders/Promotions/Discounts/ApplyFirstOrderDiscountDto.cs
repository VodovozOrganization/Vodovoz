using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация для применения скидки на первый заказ
	/// </summary>
	public class ApplyFirstOrderDiscountDto
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
	}
}
