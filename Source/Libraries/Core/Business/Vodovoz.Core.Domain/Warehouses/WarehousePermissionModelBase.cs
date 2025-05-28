using System.Collections.Generic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public abstract class WarehousePermissionModelBase
	{
		public abstract void AddOnUpdatePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse, bool? permissionValue);
		public abstract void DeletePermission(WarehousePermissionsType warehousePermissionType, Warehouse warehouse);
		public abstract IList<WarehousePermissionBase> AllPermission { get; set; }
	}
}
