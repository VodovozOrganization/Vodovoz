using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	public interface IOnlineOrderDiscountHandler : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCode onlineOrderPromoCode);
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)> TryApplyPromoCodeV7(
			IUnitOfWork uow, CanApplyOnlineOrderPromoCodeV7 receivedData);
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
			CanApplyFirstOrderDiscountRequest receivedData);
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
	}
}
