using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.EntityRepositories.Permissions
{
	public class PrivilegesRepository : IPrivilegesRepository
	{
		public IEnumerable<PrivilegeBase> GetAllPrivileges(IUnitOfWork uow) => uow.GetAll<PrivilegeBase>().ToList();
		public IEnumerable<PrivilegeName> GetAllPrivilegesNames(IUnitOfWork uow) => uow.GetAll<PrivilegeName>().ToList();
	}
}
