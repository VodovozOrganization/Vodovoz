using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Interfaces.Cart
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
