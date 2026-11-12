using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Nodes;

namespace VodovozBusiness.EntityRepositories.Sale
{
	public interface IOnlineOrderTemplateRepository
	{
		/// <summary>
		/// Получение количества "рабочих" шаблонов клиента(всех, которые не архивные, дабы исключить перебор по максимуму)
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		int GetActiveOnlineOrderTemplatesCount(IUnitOfWork uow, int counterpartyId);

		/// <summary>
		/// Существует такой не архивный шаблон
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="templateId">Идентификатор шаблона</param>
		/// <returns></returns>
		bool NotArchiveTemplateExists(IUnitOfWork uow, int templateId);

		/// <summary>
		/// Шаблон принадлежит клиенту
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="templateId">Идентификатор шаблона</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		bool OnlineOrderTemplateFromCounterparty(IUnitOfWork uow, int templateId, int? counterpartyId);

		[Obsolete]
		IEnumerable<OnlineOrderTemplate> GetActiveOnlineOrdersTemplatesForCreateOrders(IUnitOfWork uow, DateTime dateTime);
		
		/// <summary>
		/// Получение данных шаблонов автозаказов
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="templatesIds">Список идентификаторов шаблонов</param>
		/// <returns></returns>
		Task<OnlineOrdersTemplatesData> GetOnlineOrdersTemplatesDataAsync(
			IUnitOfWork uow,
			IEnumerable<int> templatesIds);
	}
}
