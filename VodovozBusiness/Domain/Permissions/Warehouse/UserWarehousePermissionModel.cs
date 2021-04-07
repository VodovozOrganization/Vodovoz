using System.Collections.Generic;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class UserWarehousePermissionModel : WarehousePermissionModel
    {
        public override void AddOnUpdatePermission(TypePermissions type, Store.Warehouse warehouse, bool permissionValue)
        {
            throw new System.NotImplementedException();
        }

        public override void DeletePermission(TypePermissions type, Store.Warehouse warehouse)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<WarehousePermission> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}