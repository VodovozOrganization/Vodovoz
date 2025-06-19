using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.EntityRepositories.Permissions
{
	public interface IUserRoleRepository
	{
		IList<AvailableDatabase> GetAllAvailableDatabases(IUnitOfWork uow);
		AvailableDatabase GetAvailableDatabaseById(IUnitOfWork uow, int id);
		IList<UserRole> GetAllUserRoles(IUnitOfWork uow);
		bool IsUserRoleWithSameNameExists(IUnitOfWork uow, string name);
		UserRole GetUserRoleById(IUnitOfWork uow, int id);
		void CreateUserRoleIfNotExists(IUnitOfWork uow, string role);
		IEnumerable<string> ShowGrantsForUser(IUnitOfWork uow, string login);
		void SetDefaultRoleToUser(IUnitOfWork uow, UserRole role, string login);
		void SetDefaultRoleToUser(IUnitOfWork uow, string role, string login);
		void GrantPrivilegeToRole(IUnitOfWork uow, string privilege, string role);
		void GrantRoleToUser(IUnitOfWork uow, string role, string login, bool withAdminOption = false);
		void RevokeRoleFromUser(IUnitOfWork uow, string role, string login);
	}
}
