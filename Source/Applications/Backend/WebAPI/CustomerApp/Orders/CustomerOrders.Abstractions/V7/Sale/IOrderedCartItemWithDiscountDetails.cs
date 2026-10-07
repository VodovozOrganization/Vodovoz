using CustomerOrders.Abstractions.V7.Carts;
using System.Collections.Generic;
using CustomerApp.Contracts.Sale;

namespace CustomerOrders.Abstractions.V7.Sale
{
	public interface IOrderedCartItemWithDiscountDetails : ICartItem
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
		/// Скидки
		/// </summary>
		IList<IDiscountAmount> Discounts { get; }
		/// <summary>
		/// Добавление фиксы
		/// </summary>
		/// <param name="fixedPrice">Фикса</param>
		void AddFixedPrice(decimal fixedPrice);
	}
}
