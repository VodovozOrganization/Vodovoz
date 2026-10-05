using System.Collections.Generic;

namespace CustomerOrders.Abstractions.V4.Sale
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
		IEnumerable<IOnlineOrderedProduct> OnlineOrderItems { get; }
	}
}
