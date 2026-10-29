using QS.DomainModel.UoW;

namespace Email.Infrastructure.Repositories
{
	public interface IDatabaseRepository
	{
		/// <summary>
		/// Получениее Id инстанса БД
		/// </summary>
		int GetCurrentDatabaseId(IUnitOfWork unitOfWork);
	}
}
