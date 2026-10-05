using System;
using System.Collections.Generic;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V5.Sale;
using CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.FixedPrice
{
	/// <summary>
	/// Информация для применения фиксы
	/// </summary>
	public class ApplyFixedPriceDto : IApplyingFixedPrice
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		/// <inheritdoc/>
		public int? ErpDeliveryPointId { get; set; }
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <inheritdoc/>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		IEnumerable<IOnlineOrderedProduct> IApplyingFixedPrice.OnlineOrderItems => OnlineOrderItems;
	}
}
