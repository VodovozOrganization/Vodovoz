using CustomerOrders.Contracts.V5.Carts;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V6.Services
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
