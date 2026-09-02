using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Фабрика по созданию классов, реализующих <see cref="IApplicablePromotion"/>
	/// </summary>
	public interface IApplicablePromotionFactory
	{
		/// <summary>
		/// Создание <see cref="ApplicablePromotion"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orderedCartItem">Позиция из корзины</param>
		/// <returns></returns>
		IApplicablePromotion CreateApplicablePromotion(
			IUnitOfWork uow,
			IOrderedCartItemWithDiscountDetails orderedCartItem);
		/// <summary>
		/// Создание <see cref="ApplicablePromotion"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="orderedCartItem">Позиция из корзины</param>
		/// <returns></returns>
		IApplicablePromotion CreateApplicablePromotion(
			IUnitOfWork uow,
			IOnlineOrderedProduct orderedCartItem);
	}
}
