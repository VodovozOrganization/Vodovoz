using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V8.Carts;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrdersApi.Library.V8.Dto.Orders.OrderItem;

namespace CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация для применения скидки на первый заказ
	/// </summary>
	public class ApplyFirstOrderDiscountDto : IApplyingFirstOrderDiscount
	{
		/// <inheritdoc/>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		/// <inheritdoc/>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		/// <inheritdoc/>
		[JsonIgnore]
		public decimal OrderSum => OnlineOrderItems.Sum(x => x.CurrentSum);

		public IEnumerable<IOrderedCartItem> CartItems => OnlineOrderItems;
	}
}
