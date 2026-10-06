using System.Collections.Generic;
using CustomerOrders.Abstractions.V8.Carts;

namespace CustomerOrders.Abstractions.V8.Sale
{
	public interface ICanCreateOnlineOrderTemplate
	{
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		int? ErpCounterpartyId { get; }
		
		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		int? DeliveryPointId { get; }
		
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
		
		/// <summary>
		/// Доставка за час
		/// </summary>
		bool IsFastDelivery { get; }
		
		/// <summary>
		/// Продаваемые позиции
		/// </summary>
		IEnumerable<ICartItem> CartItems { get; }
	}
}
