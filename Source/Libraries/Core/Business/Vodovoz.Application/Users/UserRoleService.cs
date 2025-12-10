using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Users;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Application.Users
{
	internal sealed class UserRoleService : IUserRoleService
	{
		private readonly GrantsRoleParser _grantsRoleParser;
		private readonly IUserRoleRepository _userRoleRepository;

		public UserRoleService(
			GrantsRoleParser grantsRoleParser,
			IUserRoleRepository userRoleRepository)
		{
			_grantsRoleParser = grantsRoleParser ?? throw new ArgumentNullException(nameof(grantsRoleParser));
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
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

		public IDictionary<string, IDictionary<string, IList<string>>> GetPrivilegesFromRoleInDatabase(IUnitOfWork uow, string userRole)
		{
			var grants = _userRoleRepository.ShowGrantsForRole(uow, userRole);
			return _grantsRoleParser.Parse(grants);
		}

		public string SearchingPatternFromUserGrants(string login) => $"GRANT [`|']?(\\w+)[`|']? TO [`|']?{login}[`|']?@[`|']?%[`|']?";
	}
}
