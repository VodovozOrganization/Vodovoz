using CustomerOrders.Abstractions.V7.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Factories
{
	/// <summary>
	/// Фабрика по созданию классов, реализующих <see cref="IApplicablePromotion"/>
	/// </summary>
	internal interface IApplicablePromotionFactory
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
	}
}
