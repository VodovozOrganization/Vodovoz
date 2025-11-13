using System.Collections.Generic;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouses
{
	public interface IWarehousePermissionValidator
	{
		bool Validate(WarehousePermissionsType warehousePermissionType, Warehouse warehouse, Employee employee);
		bool Validate(Employee employee, WarehousePermissionsType warehousePermissionType, int warehouseId);
		IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissionsType permissionType, Employee employee);
	}
}
