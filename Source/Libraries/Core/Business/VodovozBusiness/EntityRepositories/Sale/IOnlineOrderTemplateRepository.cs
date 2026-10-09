using QS.DomainModel.UoW;

namespace VodovozBusiness.EntityRepositories.Sale
{
	public interface IOnlineOrderTemplateRepository
	{
		/// <summary>
		/// Получение количества активных шаблонов клиента
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		int GetActiveOnlineOrderTemplatesCount(IUnitOfWork uow, int counterpartyId);
	}
}
