using System;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V5.Carts;
using CustomerOrders.Contracts.V5.Orders.Discounts;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <summary>
	/// Контракт создания условий по автозаказу
	/// </summary>
	public interface IOnlineOrderTemplateConditionsCreator
	{
		/// <summary>
		/// Создание условий по автозаказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="checkId">Идентификатор проверки корзины</param>
		/// <param name="discount">Параметры скидки</param>
		/// <returns></returns>
		Task<OnlineAutoOrderConditions> CreateAsync(IUnitOfWork uow, Guid? checkId, DiscountDto discount);
	}
}
