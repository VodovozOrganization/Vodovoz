using System;
using System.Collections.Generic;
using CustomerApp.Contracts.Sale;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Orders.Items;

namespace CustomerOrders.Contracts.V8.Sale
{
	public class ApplyAutoOrderDiscountRequest : ICanCreateOnlineOrderTemplate
	{
		/// <summary>
		/// ИПЗ
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		
		/// <inheritdoc/>
		public int? DeliveryPointId { get; set; }
		
		/// <inheritdoc/>
		public bool IsSelfDelivery { get; set; }

		/// <inheritdoc/>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Идентификатор заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Применить или снять скидку
		/// </summary>
		public bool Apply { get; set; }

		/// <summary>
		/// Текущее наполнение корзины
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		
		IEnumerable<ICartItem> ICanCreateOnlineOrderTemplate.CartItems => OnlineOrderItems;
	}
}
