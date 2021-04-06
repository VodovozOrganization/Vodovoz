using System.Collections.Generic;
namespace Vodovoz.Domain.Permissions.Warehouse
{
	public interface IWarehousePermissionValidator
	{
		bool Validate(WarehousePermissions warehousePermission, Store.Warehouse warehouse);
		bool Validate(WarehousePermissions warehousePermission, int warehouseId);
		IEnumerable<Store.Warehouse> GetAllowedWarehouses(WarehousePermissions permission);
	}
}
