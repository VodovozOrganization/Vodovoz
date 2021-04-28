using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;
namespace Vodovoz.Core
{
	public class WarehousePermissionValidatorFactory : IWarehousePermissionValidatorFactory
	{
		// public IWarehousePermissionValidator CreateValidator(int userId)
		// {
		// 	PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix;
		// 	using(var uow = UnitOfWorkFactory.CreateForRoot<User>(userId)) {
		// 		permissionMatrix = new PermissionMatrix<WarehousePermissions, Warehouse>();
		// 		permissionMatrix.Init();
		// 		permissionMatrix.ParseJson(uow.Root.WarehouseAccess);
		// 	}
		// 	return new WarehousePermissionValidator(permissionMatrix);
		// }

		public IWarehousePermissionValidator CreateValidator(int SubdivisionId)
		{
			IEnumerable<SubdivisionWarehousePermission> warehousePermission;
			using (var uow = UnitOfWorkFactory.CreateForRoot<Subdivision>(SubdivisionId))
			{
				warehousePermission = uow.Session.QueryOver<SubdivisionWarehousePermission>().Select(x=>x.Subdivision.Id == SubdivisionId).List();
			}

			return new WarehousePermissionValidator(warehousePermission);
		}
	}
}
