using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Domain.Users;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Application.Users
{
	internal sealed class UserRoleService : IUserRoleService
	{
		private readonly IUserRoleRepository _userRoleRepository;

		public UserRoleService(IUserRoleRepository userRoleRepository)
		{
			_userRoleRepository = userRoleRepository
				?? throw new ArgumentNullException(nameof(userRoleRepository));
		}

		public void GrantDatabasePrivileges(IUnitOfWork uow, UserRole userRole)
		{
			if(string.IsNullOrWhiteSpace(userRole.Name))
			{
				return;
			}

			foreach(var privilege in userRole.Privileges)
			{
				_userRoleRepository.GrantPrivilegeToRole(uow, privilege.ToString(), userRole.Name);
			}
		}

		public string SearchingPatternFromUserGrants(string login) => $"GRANT [`|']?(\\w+)[`|']? TO [`|']?{login}[`|']?@[`|']?%[`|']?";
	}
}
