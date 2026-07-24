using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Cart;

namespace Vodovoz.Core.Domain.Interfaces.Sale
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
		/// Скидки
		/// </summary>
		IList<int> DiscountIds { get; }
		void AddFixedPrice(decimal fixedPrice);
	}
}
