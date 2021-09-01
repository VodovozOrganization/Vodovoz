using System.Collections.Generic;

namespace Vodovoz.Domain.Permissions.Warehouses
{
    public abstract class WarehousePermissionModelBase
    {
        public abstract void AddOnUpdatePermission(WarehousePermissionsType warehousePermissionType, Store.Warehouse warehouse, bool? permissionValue);
        public abstract void DeletePermission(WarehousePermissionsType warehousePermissionType, Store.Warehouse warehouse);

        public abstract IEnumerable<WarehousePermission> GetEnumerator();
        
        public abstract List<WarehousePermission> AllPermission { get; set; }
    }
}