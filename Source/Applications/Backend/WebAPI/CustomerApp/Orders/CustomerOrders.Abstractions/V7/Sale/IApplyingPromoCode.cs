using CustomerOrders.Abstractions.V7.Carts;
using System;
using System.Collections.Generic;

namespace CustomerOrders.Abstractions.V7.Sale
{
	public interface IApplyingPromoCode
	{
		/// <summary>
		/// Источник
		/// </summary>
		ExternalSource Source { get; }
		/// <summary>
		/// Время, когда пришел запрос
		/// </summary>
		DateTime RequestTime { get; }
		/// <summary>
		/// Id клиента
		/// </summary>
		int? ErpCounterpartyId { get; }
		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		IEnumerable<IOrderedCartItem> OnlineOrderItems { get; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		decimal OrderSum { get; }
		/// <summary>
		/// Промокод
		/// </summary>
		string PromoCode { get; }
	}
}
