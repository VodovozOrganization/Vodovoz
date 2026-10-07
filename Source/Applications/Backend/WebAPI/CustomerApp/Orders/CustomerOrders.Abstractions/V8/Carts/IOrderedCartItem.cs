using System.Collections.Generic;
using CustomerApp.Contracts.Sale;

namespace CustomerOrders.Abstractions.V8.Carts
{
	public interface IOrderedCartItem : ICartItem
	{
		/// <summary>
		/// Цена(по прайсу)
		/// </summary>
		decimal Price { get; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		decimal CurrentPrice { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма со скидкой
		/// </summary>
		decimal CurrentSum { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; set; }
		/// <summary>
		/// Идентификаторы скидок
		/// </summary>
		IEnumerable<int> DiscountIds { get; }
	}
}
