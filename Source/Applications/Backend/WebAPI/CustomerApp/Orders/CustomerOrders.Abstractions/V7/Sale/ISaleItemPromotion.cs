using System.Collections.Generic;
using CustomerOrders.Abstractions.Common;

namespace CustomerOrders.Abstractions.V7.Sale
{
	/// <summary>
	/// Данные по примененной акции/скидке/фиксе
	/// </summary>
	public interface ISaleItemPromotion
	{
		/// <summary>
		/// Успешное выполнение
		/// </summary>
		bool Ok { get; }
		/// <summary>
		/// Сообщение
		/// </summary>
		string Message { get; }
		/// <summary>
		/// Информационное сообщение
		/// </summary>
		IInfoMessage Warning { get; }
		/// <summary>
		/// Список товаров с примененной акцией/скидкой/фиксой
		/// </summary>
		IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems { get; }
	}
}
