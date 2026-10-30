using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Common;
using CustomerOrders.Contracts.V8.Sale.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
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

		/// <summary>
		/// Получение детализированной информации по шаблону автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="source">Источник запроса</param>
		/// <param name="templateId">Идентификатор шаблона автозаказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<OrderTemplateInfoDto> CreateOrderTemplateInfoAsync(
			IUnitOfWork uow,
			ExternalSource source,
			int templateId,
			CancellationToken cancellationToken
			);
	}
}
