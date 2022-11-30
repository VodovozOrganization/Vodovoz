using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public interface IWarehousePermissionValidator
	{
		bool Validate(WarehousePermissionsType warehousePermissionType, Store.Warehouse warehouse, User user);
		bool Validate(WarehousePermissionsType warehousePermissionType, int warehouseId);
		IEnumerable<Store.Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Employee employee);
	}
}
