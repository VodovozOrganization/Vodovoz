using System.Collections;
using System.Collections.Generic;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public abstract class WarehousePermissionModel
    {
        public abstract void AddOnUpdatePermission(TypePermissions type, Store.Warehouse warehouse, bool permissionValue);
        public abstract void DeletePermission(TypePermissions type, Store.Warehouse warehouse);

        public abstract IEnumerable<WarehousePermission> GetEnumerator();
    }
}