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
	/// Информация для проверки применимости промокода
	/// </summary>
	public class ApplyPromoCodeDto : IApplyingPromoCode
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		[JsonIgnore]
		public decimal OrderSum => OnlineOrderItems.Sum(x => x.CurrentSum);
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		[JsonIgnore]
		public DateTime RequestTime { get; } = DateTime.UtcNow;
		IEnumerable<IOrderedCartItem> IApplyingPromoCode.OnlineOrderItems => OnlineOrderItems;
	}
}
