using System.Collections.Generic;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Controllers
{
	public interface IUserPermissionsController
	{
		void AddingPermissionsToUser(IUnitOfWork uow, int fromUserId, int toUserId);
		void ChangePermissionsFromUser(IUnitOfWork uow, int fromUserId, int toUserId);
		IList<UserPermissionNode> GetAllNewEntityUserPermissions();
		IList<WarehousePermissionBase> NewUserWarehousesPermissions { get; }
		IList<HierarchicalPresetPermissionBase> NewUserPresetPermissions { get; }
		IList<EntityUserPermission> NewEntityUserPermissions { get; }
		IList<EntityUserPermissionExtended> NewEntityUserPermissionsExtended { get; }
		IList<EntitySubdivisionForUserPermission> NewEntitySubdivisionForUserPermissions { get; }
	}
}
