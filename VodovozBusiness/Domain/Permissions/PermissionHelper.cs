using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Repositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public static class PermissionHelper
	{
		public static EntityPermission GetPermissionForSubdivision(Subdivision subdivision, string entityName)
		{
			EntityPermission result = EntityPermission.Empty;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				EntitySubdivisionPermission permission = null;
				Subdivision currentSubdivision = subdivision;
				while(currentSubdivision != null) {
					permission = PermissionRepository.GetSubdivisionEntityPermission(uow, entityName, currentSubdivision.Id);
					if(permission != null) {
						result = new EntityPermission(
							permission.CanCreate,
							permission.CanRead,
							permission.CanUpdate,
							permission.CanDelete);
						break;
					}
					currentSubdivision = currentSubdivision.ParentSubdivision;
				}
			}
			return result;
		}
	}
}
