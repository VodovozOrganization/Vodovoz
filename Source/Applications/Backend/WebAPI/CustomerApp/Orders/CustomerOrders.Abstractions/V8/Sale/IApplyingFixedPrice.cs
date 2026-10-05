using System.Collections.Generic;
using CustomerOrders.Abstractions.V8.Carts;

namespace CustomerOrders.Abstractions.V8.Sale
{
	public interface IApplyingFixedPrice
	{
		/// <summary>
		/// Id клиента
		/// </summary>
		int? ErpCounterpartyId { get; }
		/// <summary>
		/// Id точки доставки
		/// </summary>
		int? ErpDeliveryPointId { get; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		bool IsSelfDelivery { get; }
		/// <summary>
		/// Список товаров
		/// </summary>
		IEnumerable<IOrderedCartItem> OnlineOrderItems { get; }
	}
}
