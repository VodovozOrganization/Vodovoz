using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Users.Settings;

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
		void GiveSelectPrivilegesToArchiveDataBase(IUnitOfWork uow, string login);
		void ChangePasswordForUser(IUnitOfWork uow, string login, string password);
		void CreateUser(IUnitOfWork uow, string login, string password);
		void DropUser(IUnitOfWork uow, string login);
		void GrantPrivilegesToUser(IUnitOfWork uow, string privileges, string tableName, string login);
		void GrantPrivilegesToNewUser(IUnitOfWork uow, string tableName, string login);
	}
}
