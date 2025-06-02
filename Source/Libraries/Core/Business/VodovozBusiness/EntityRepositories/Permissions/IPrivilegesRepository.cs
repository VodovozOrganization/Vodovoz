using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.EntityRepositories.Permissions
{
	public interface IPrivilegesRepository
	{
		IEnumerable<PrivilegeBase> GetAllPrivileges(IUnitOfWork uow);
		IEnumerable<PrivilegeName> GetAllPrivilegesNames(IUnitOfWork uow);
	}
}
