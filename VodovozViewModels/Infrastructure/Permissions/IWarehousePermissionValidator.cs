using System;
using System.Collections.Generic;
using Vodovoz.Domain.Store;
namespace Vodovoz.Infrastructure.Permissions
{
	public interface IWarehousePermissionValidator
	{
		bool Validate(WarehousePermissions warehousePermission, Warehouse warehouse);
		bool Validate(WarehousePermissions warehousePermission, int warehouseId);
		IEnumerable<Warehouse> GetAllowedWarehouses(WarehousePermissions permission);
	}
}
