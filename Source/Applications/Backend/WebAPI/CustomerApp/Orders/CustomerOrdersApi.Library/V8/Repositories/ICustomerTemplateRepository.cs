using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Dto.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Repositories
{
	public interface ICustomerTemplateRepository
	{
		/// <summary>
		/// Получение интервалов доставки по дням
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderId">Идентификатор онлайн заказа</param>
		/// <returns></returns>
		ILookup<WeekDayName, DeliveryScheduleDto> GetWeeklyDeliverySchedules(IUnitOfWork uow, int onlineOrderId);

		/// <summary>
		/// Получение данных для проверки возможности создания автозаказа из онлайн заказа <see cref="CanCreateTemplateDto"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderId">Идентификатор онлайн заказа</param>
		/// <returns></returns>
		CanCreateTemplateDto GetCanCreateTemplateDataFromOnlineOrder(IUnitOfWork uow, int onlineOrderId);

		/// <summary>
		/// Получение данных для формирования ответа по детализации автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="templateId">Идентификатор шаблона автозаказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные для формирования ответа по детализации автозаказа <see cref="OrderTemplateInfoRawData"/></returns>
		Task<OrderTemplateInfoRawData> GetOrderTemplateInfoAsync(
			IUnitOfWork uow,
			int templateId,
			CancellationToken cancellationToken
			);

		/// <summary>
		/// Получение списка шаблонов автозаказов клиента, не архивных
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="erpCounterpartyId">Идентификатор клиента</param>
		/// <param name="skipElements">Пропустить количество, если грузится не первая страница</param>
		/// <param name="templatesCountOnPage">Загружаемое количество(отображаемое на странице)</param>
		/// <returns></returns>
		Task<OrderTemplateInfoDto> GetOrderTemplatesAsync(
			IUnitOfWork uow,
			int erpCounterpartyId,
			int skipElements,
			int templatesCountOnPage
			);
	}
}
