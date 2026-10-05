using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Контракт создания условий по автозаказу
	/// </summary>
	public interface IOnlineOrderTemplateConditionsCreator
	{
		/// <summary>
		/// Данные скидки для автозаказа
		/// </summary>
		DiscountDto DiscountData { get; }
		/// <summary>
		/// Создание условий по автозаказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="request">Данные запроса</param>
		/// <returns></returns>
		OnlineAutoOrderConditions Create(IUnitOfWork uow, OrderConditionsRequest request);
	}
}
