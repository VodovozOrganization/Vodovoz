using System.Collections.Generic;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public abstract class WarehousePermissionModel
    {
        public abstract void AddOnUpdatePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse, bool? permissionValue);
        public abstract void DeletePermission(WarehousePermissions warehousePermission, Store.Warehouse warehouse);

        public abstract IEnumerable<WarehousePermission> GetEnumerator();
        
        public abstract List<WarehousePermission> AllPermission { get; set; }
    }
}