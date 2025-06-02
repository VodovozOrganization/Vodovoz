using QS.DomainModel.UoW;

namespace Vodovoz.Core.Domain.Users
{
	public interface IUserRoleService
	{
		void GrantDatabasePrivileges(IUnitOfWork uow, UserRole userRole);
		string SearchingPatternFromUserGrants(string login);
	}
}
