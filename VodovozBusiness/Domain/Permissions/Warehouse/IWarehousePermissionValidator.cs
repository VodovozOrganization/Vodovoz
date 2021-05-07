using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions.Warehouse
{
	public interface IWarehousePermissionValidator
	{
		bool Validate(WarehousePermissions warehousePermission, Store.Warehouse warehouse, User user);
		bool Validate(WarehousePermissions warehousePermission, int warehouseId);
		IEnumerable<Store.Warehouse> GetAllowedWarehouses(WarehousePermissions permission, Subdivision subdivision);
	}
}
