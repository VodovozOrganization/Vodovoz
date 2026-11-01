using QS.DomainModel.UoW;

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
		bool OnlineOrderFromCounterparty(IUnitOfWork uow, int templateId, int? counterpartyId);
	}
}
