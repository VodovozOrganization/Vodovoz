using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Common;

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
		IInfoMessage Warning { get; }
		/// <summary>
		/// Список товаров с примененной акцией/скидкой/фиксой
		/// </summary>
		IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems { get; }
	}
}
