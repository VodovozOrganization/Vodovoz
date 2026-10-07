using CustomerApp.Contracts.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders.Cart
{
	/// <summary>
	/// Интерфейс промонабора из корзины ИПЗ
	/// </summary>
	public interface IPromoSetCartItem : ICartItemBase
	{
		/// <summary>
		/// Промонабор
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
}
