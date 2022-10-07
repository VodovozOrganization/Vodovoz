using System;
using System.Collections.Generic;
using QS.Permissions;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Permissions;

namespace Vodovoz.Core
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix;

		public WarehousePermissionValidator(PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix)
		{
			this.permissionMatrix = permissionMatrix ?? throw new ArgumentNullException(nameof(permissionMatrix));
		}

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissions permission)
		{
			return permissionMatrix.Allowed(permission);
		}

		public bool Validate(WarehousePermissions warehousePermission, Warehouse warehouse)
		{
			if(warehouse == null) {
				return false;
			}

			return Validate(warehousePermission, warehouse.Id);
		}

		public bool Validate(WarehousePermissions warehousePermission, int warehouseId)
		{
			return permissionMatrix[warehousePermission, warehouseId];
		}
	}
}
