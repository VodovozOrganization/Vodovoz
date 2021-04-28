using System;
using System.Collections.Generic;
using System.Linq;
using QS.Permissions;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.Core
{
	public class WarehousePermissionValidator : IWarehousePermissionValidator
	{
		private readonly PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix;
		private readonly IEnumerable<SubdivisionWarehousePermission> subdivisionWarehousePermissions;
		public WarehousePermissionValidator(PermissionMatrix<WarehousePermissions, Warehouse> permissionMatrix)
		{
			this.permissionMatrix = permissionMatrix ?? throw new ArgumentNullException(nameof(permissionMatrix));
		}

		public WarehousePermissionValidator(IEnumerable<SubdivisionWarehousePermission> subdivisionWarehousePermissions)
		{
			this.subdivisionWarehousePermissions = subdivisionWarehousePermissions;
		}

		public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissions permissions)
		{
			return subdivisionWarehousePermissions.Where(x => x.WarehousePermissionType == permissions)
				.Select(x => x.Warehouse);
		}

		// public IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissions permission)
		// {
		// 	return permissionMatrix.Allowed(permission);
		// }

		public bool Validate(WarehousePermissions warehousePermissions, Warehouse warehouse)
		{
			if (warehouse is null) return false;
			return Validate(warehousePermissions, warehouse.Id);
		}
		// public bool Validate(WarehousePermissions warehousePermission, Warehouse warehouse)
		// {
		// 	if(warehouse == null) {
		// 		return false;
		// 	}
		//
		// 	return Validate(warehousePermission, warehouse.Id);
		// }

		public bool Validate(WarehousePermissions warehousePermissions, int warehouseId)
			=> subdivisionWarehousePermissions.SingleOrDefault(x =>
					x.Warehouse.Id == warehouseId && x.WarehousePermissionType == warehousePermissions).ValuePermission
				.Value;

		// public bool Validate(WarehousePermissions warehousePermission, int warehouseId)
		// {
		// 	return permissionMatrix[warehousePermission, warehouseId];
		// }
	}
}
