using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Interfaces.Sale
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
		/// Список товаров с примененной акцией/скидкой/фиксой
		/// </summary>
		IEnumerable<IOrderedCartItem> SaleItems { get; }
	}
}
