using CustomerOrders.Contracts.V8.Sale.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V8.Factories
{
	public interface ICustomerTemplateFactory
	{
		/// <summary>
		/// Создание ответа по проверке параметров автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="request">Данные запроса</param>
		/// <returns></returns>
		CheckOrderTemplateResponse CreateCheckOrderTemplateResponse(
			IUnitOfWork uow,
			CheckOrderTemplateRequest request);

		/// <summary>
		/// Создание ответа по созданию шаблона автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="request">Данные запроса</param>
		/// <returns></returns>
		CreateOrderTemplateResponse CreateOrderTemplateResponse(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			CreateOrderTemplateRequest request);
	}
}
