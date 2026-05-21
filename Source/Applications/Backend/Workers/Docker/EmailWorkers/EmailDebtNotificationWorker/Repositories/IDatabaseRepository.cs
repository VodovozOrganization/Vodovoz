using QS.DomainModel.UoW;

namespace EmailDebtNotificationWorker.Repositories
{
	public interface IDatabaseRepository
	{
		/// <summary>
		/// Получениее Id инстанса БД
		/// </summary>
		int GetCurrentDatabaseId(IUnitOfWork unitOfWork);
	}
}
