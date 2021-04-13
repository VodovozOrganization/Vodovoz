using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public abstract class WarehousePermissionNodeViewModel
    {
        public WarehousePermissionModel WarehousePermissionModel { get; set; }
        
        public WarehousePermissions WarehousePermissions { get; set; }
        
        public Warehouse Warehouse { get; set; }
        
        public bool? PermissionValue { get; set; }
    }
}