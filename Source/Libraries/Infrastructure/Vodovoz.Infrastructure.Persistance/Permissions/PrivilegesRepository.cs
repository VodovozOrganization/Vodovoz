using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Infrastructure.Persistance.Permissions
{
	internal sealed class PrivilegesRepository : IPrivilegesRepository
	{
		public IEnumerable<PrivilegeBase> GetAllPrivileges(IUnitOfWork uow) => uow.GetAll<PrivilegeBase>().ToList();
		public IEnumerable<PrivilegeName> GetAllPrivilegesNames(IUnitOfWork uow) => uow.GetAll<PrivilegeName>().ToList();
	}
}
