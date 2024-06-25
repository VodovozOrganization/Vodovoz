using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Infrastructure.Persistance.Permissions
{
	public class PrivilegesRepository : IPrivilegesRepository
	{
		public IEnumerable<PrivilegeBase> GetAllPrivileges(IUnitOfWork uow) => uow.GetAll<PrivilegeBase>().ToList();
		public IEnumerable<PrivilegeName> GetAllPrivilegesNames(IUnitOfWork uow) => uow.GetAll<PrivilegeName>().ToList();
	}
}
