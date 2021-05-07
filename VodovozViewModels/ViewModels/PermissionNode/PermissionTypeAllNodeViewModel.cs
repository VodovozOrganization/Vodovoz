using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class PermissionTypeAllNodeViewModel : WarehousePermissionAllNodeViewModelBase
    {
        private WarehousePermissions warehousePermissions;
        public WarehousePermissions WarehousePermissions
        {
            get => warehousePermissions;
            set => SetField(ref warehousePermissions, value);
        }

        public PermissionTypeAllNodeViewModel(WarehousePermissions warehousePermissions, IEnumerable<Warehouse> warehouses, WarehousePermissionModel warehousePermissionModel)
        {
            WarehousePermissions = warehousePermissions;
            Title = WarehousePermissions.GetEnumTitle();
            SubNodeViewModel = new List<WarehousePermissionNodeViewModel>();
            foreach (var warehouse in warehouses)
            {
                var permissionNode = new WarehousePermissionNodeViewModel(warehouse, warehousePermissions, warehousePermissionModel);
                permissionNode.ItemChangeValue += IsAllPermissionSeted;
                SubNodeViewModel.Add(permissionNode);
            }
        }
        
        public void IsAllPermissionSeted(object sender, EventArgs e)
        {
            var warehousePermissionNodeViewModel = sender as WarehousePermissionNodeViewModel;
            if (warehousePermissionNodeViewModel.UnSubscribe) return;
            UnSetAll = true;
            var collection = SubNodeViewModel.Where(x => x.Warehouse !=
                warehousePermissionNodeViewModel.Warehouse);
            if (collection.All(x => x.PermissionValue == true) && warehousePermissionNodeViewModel.PermissionValue == true)
            {
                PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
            }
            else if (collection.All(x => x.PermissionValue == false) &&
                 warehousePermissionNodeViewModel.PermissionValue == false)
            {
                PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
            }
            else PermissionValue = null;

            UnSetAll = false;
        }
    }
}