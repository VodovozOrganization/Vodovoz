using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories
{
	public interface IUserRepository
	{
		User GetCurrentUser(IUnitOfWork uow);
		User GetUserById(IUnitOfWork uow, int id);
		UserSettings GetCurrentUserSettings(IUnitOfWork uow);
		string GetTempDirForCurrentUser(IUnitOfWork uow);
		UserSettings GetUserSettings(IUnitOfWork uow, int userId);
		User GetUserByLogin(IUnitOfWork uow, string login);
		bool MySQLUserWithLoginExists(IUnitOfWork uow, string login);
		void GiveSelectPrivelegesToArchiveDataBase(IUnitOfWork uow, string login);
	}
}
