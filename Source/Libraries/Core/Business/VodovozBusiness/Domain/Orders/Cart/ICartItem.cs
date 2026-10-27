using Vodovoz.Core.Domain.Goods;

namespace VodovozBusiness.Domain.Orders.Cart
{
	/// <summary>
	/// Интерфейс товара из корзины ИПЗ
	/// </summary>
	public interface ICartItem
	{
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Тип позиции корзины
		/// </summary>
		SaleItemType ItemType { get; }
	}
}
