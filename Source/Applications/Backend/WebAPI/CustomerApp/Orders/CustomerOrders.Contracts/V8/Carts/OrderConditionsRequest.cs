using CustomerOrders.Abstractions;
using CustomerOrders.Contracts.V8.Orders.Items;
using System;
using System.Collections.Generic;
using CustomerOrders.Abstractions.V8.Carts;
using CustomerOrders.Abstractions.V8.Sale;

namespace CustomerOrders.Contracts.V8.Carts
{
	/// <summary>
	/// Запрос условий доставки
	/// </summary>
	public sealed class OrderConditionsRequest : ICanCreateOnlineOrderTemplate
	{
		private IEnumerable<OnlineOrderItemDto> _onlineOrderItems;

		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
		
		/// <summary>
		/// Идентификатор проверки
		/// </summary>
		public Guid? CheckId { get; set; }
		
		/// <inheritdoc/>
		public int? ErpCounterpartyId { get; set; }
		
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		
		/// <inheritdoc/>
		public int? DeliveryPointId { get; set; }
		
		/// <inheritdoc/>
		public bool IsSelfDelivery { get; set; }
		
		/// <inheritdoc/>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		public int? SelfDeliveryGeoGroupId { get; set; }
		
		/// <summary>
		/// Продаваемые позиции
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems
		{
			get
			{
				if(_onlineOrderItems is null)
				{
					_onlineOrderItems = new List<OnlineOrderItemDto>();
				}
				return _onlineOrderItems;
			}
			
			set => _onlineOrderItems = value;
		}

		IEnumerable<ICartItem> ICanCreateOnlineOrderTemplate.CartItems => OnlineOrderItems;
	}
}
