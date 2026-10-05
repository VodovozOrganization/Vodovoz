using CustomerOrders.Abstractions.V7.Carts;
using CustomerOrders.Abstractions.V7.Sale;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.Controllers;
using VodovozBusiness.Nodes;

namespace CustomerOrdersApi.Library.V7.Services
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	internal interface IOnlineOrderDiscountHandler : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)> TryApplyPromoCode(
			IUnitOfWork uow, IApplyingPromoCode receivedData);
		/// <summary>
		/// Расчет детализированных скидок позиции 
		/// </summary>
		/// <param name="receivedCartItem">Позиция из корзины</param>
		/// <param name="discountReasons">Основания скидок из позиции</param>
		void CalculateDiscount(
			IOrderedCartItemWithDiscountDetails receivedCartItem,
			IEnumerable<DiscountReasonBase> discountReasons
		);
		/// <summary>
		/// Применение скидки на первый заказ к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки промокода и товары <see cref="CanApplyFirstOrderDiscountRequest"/></param>
		/// <returns></returns>
		IEnumerable<IOrderedCartItemWithDiscountDetails> TryApplyFirstOrderDiscount(
			IUnitOfWork uow,
			IApplyingFirstOrderDiscount receivedData);
		/// <summary>
		/// Возврат списка товаров с рассчитанными скидками, если они есть
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="cartItems">Список товаров</param>
		/// <returns></returns>
		IEnumerable<IOrderedCartItemWithDiscountDetails> CalculateDiscounts(
			IUnitOfWork uow,
			IEnumerable<IOrderedCartItem> cartItems
		);
		/// <summary>
		/// Расчет детализации скидки в деньгах по основаниям скидки, включая персональную скидку
		/// Подходит для случаев, когда надо раскрыть информацию по скидкам из заказа или онлайн заказа
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		(decimal TotalDiscount, IDictionary<int, IDiscountAmount> DiscountDetails) CalculateTotalDiscountDetails(
			ICalculatingTotalMoneyDiscount saleItem
		);
	}
}
