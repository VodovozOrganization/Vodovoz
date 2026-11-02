using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders.Cart
{
	/// <summary>
	/// Интерфейс промонабора из корзины ИПЗ
	/// </summary>
	public interface IPromoSetCartItem : ICartItem
	{
		/// <summary>
		/// Промонабор
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
}
