using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V7.Carts;
using CustomerOrders.Abstractions.V7.Sale;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация для проверки применимости промокода
	/// </summary>
	public class ApplyPromoCodeDto : IApplyingPromoCode
	{
		/// <inheritdoc/>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <inheritdoc/>
		public string PromoCode { get; set; }
		/// <inheritdoc/>
		[JsonIgnore]
		public decimal OrderSum => OnlineOrderItems.Sum(x => x.CurrentSum);
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		/// <inheritdoc/>
		[JsonIgnore]
		public DateTime RequestTime { get; } = DateTime.UtcNow;

		IEnumerable<IOrderedCartItem> IApplyingPromoCode.OnlineOrderItems => OnlineOrderItems;
	}
}
