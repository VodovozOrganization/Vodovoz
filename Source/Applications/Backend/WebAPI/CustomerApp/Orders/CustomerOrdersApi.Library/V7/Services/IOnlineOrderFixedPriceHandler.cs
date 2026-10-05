using System.Collections.Generic;
using CustomerOrders.Abstractions.V7.Sale;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V7.Services
{
	/// <summary>
	/// Интерфейс для работы с фиксой в ИПЗ
	/// </summary>
	internal interface IOnlineOrderFixedPriceHandler
	{
		/// <summary>
		/// Применение фиксы к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки фиксы и товары
		/// <see cref="IApplyingFixedPrice"/></param>
		/// <returns></returns>
		(bool? AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems) TryApplyFixedPrice(
			IUnitOfWork uow, IApplyingFixedPrice receivedData);
	}
}
