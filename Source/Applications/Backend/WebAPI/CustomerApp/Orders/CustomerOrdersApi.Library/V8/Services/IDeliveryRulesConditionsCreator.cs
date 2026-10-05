using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Создатель доп условий по доставке
	/// </summary>
	public interface IDeliveryRulesConditionsCreator
	{
		/// <summary>
		/// Создание доп условий по доставке для проверок в корзине
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="request">Данные запроса</param>
		/// <returns></returns>
		DeliveryRulesConditions Create(IUnitOfWork uow, OrderConditionsRequest request);
	}
}
