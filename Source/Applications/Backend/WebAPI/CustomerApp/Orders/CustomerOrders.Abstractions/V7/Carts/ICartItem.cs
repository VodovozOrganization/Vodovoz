using CustomerOrders.Abstractions.V7.Sale;

namespace CustomerOrders.Abstractions.V7.Carts
{
	public interface ICartItem
	{
		/// <summary>
		/// Id товара/услуги в ДВ
		/// </summary>
		int ErpId { get; }
		/// <summary>
		/// Тип товара/услуги
		/// </summary>
		SaleItemType ItemType { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
	}
}
