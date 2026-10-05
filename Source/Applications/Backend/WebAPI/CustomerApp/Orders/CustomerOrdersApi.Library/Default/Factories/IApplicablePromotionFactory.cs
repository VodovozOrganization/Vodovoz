using CustomerOrders.Abstractions.V3.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Factories
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
			IOnlineOrderedProduct orderedCartItem);
	}
}
