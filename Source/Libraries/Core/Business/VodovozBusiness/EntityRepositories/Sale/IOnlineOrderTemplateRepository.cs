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
	}
}
