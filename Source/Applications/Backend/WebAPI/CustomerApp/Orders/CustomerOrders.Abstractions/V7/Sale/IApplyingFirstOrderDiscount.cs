using CustomerOrders.Abstractions.V7.Carts;
using System;
using System.Collections.Generic;

namespace CustomerOrders.Abstractions.V7.Sale
{
	public interface IApplyingFirstOrderDiscount
	{
		/// <summary>
		/// Источник
		/// </summary>
		ExternalSource Source { get; }

		/// <summary>
		/// Id клиента
		/// </summary>
		int? ErpCounterpartyId { get; }

		/// <summary>
		/// Идентификатор пользователя ИПЗ
		/// </summary>
		Guid? ExternalCounterpartyId { get; }

		/// <summary>
		/// Товары онлайн заказа
		/// </summary>
		IEnumerable<IOrderedCartItem> CartItems { get; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		decimal OrderSum { get; }
	}
}
