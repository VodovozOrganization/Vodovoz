using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehouseAllNodeViewModel : WarehousePermissionAllNodeViewModelBase
    {
        private Warehouse warehouse;
        public Warehouse Warehouse
        {
            get => warehouse;
            set => SetField(ref warehouse, value);
        }
        public WarehouseAllNodeViewModel(Warehouse warehouse, IEnumerable<WarehousePermissions> permissionTypes, WarehousePermissionModel warehousePermissionModel)
        {
            this.warehouse = warehouse;
            Title = warehouse.Name;
            
            SubNodeViewModel = new List<WarehousePermissionNodeViewModel>();
            foreach (var permission in permissionTypes)
            {
                var warehouseNode = new WarehousePermissionNodeViewModel(Warehouse, permission, warehousePermissionModel);
                warehouseNode.ItemChangeValue += IsAllWarehousesSeted;
                SubNodeViewModel.Add(warehouseNode);
            }
        }

        public void IsAllWarehousesSeted(object sender, EventArgs e)
        {
            var warehousePermissionNodeViewModel = sender as WarehousePermissionNodeViewModel;
            if(warehousePermissionNodeViewModel.UnSubscribe) return;
            UnSetAll = true;
            var collection = SubNodeViewModel.Where(x => x.WarehousePermissions !=
                                                         warehousePermissionNodeViewModel.WarehousePermissions);
            if (collection.All(x => x.PermissionValue == true) && warehousePermissionNodeViewModel.PermissionValue == true)
            {
                PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
            }
            else if (collection.All(x=>x.PermissionValue == false) && warehousePermissionNodeViewModel.PermissionValue == false)
            {
                PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
            }
            else PermissionValue = null;

            UnSetAll = false;
        }
    }
}