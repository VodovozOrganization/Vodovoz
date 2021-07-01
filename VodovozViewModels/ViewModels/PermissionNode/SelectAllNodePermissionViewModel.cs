using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class SelectAllNodePermissionViewModel : PropertyChangedBase
    {
        private string title;
        public string Title
        {
            get => title; 
            set => SetField(ref title, value);
        }
        
        public SelectAllNodePermissionViewModel(List<WarehouseAllNodeViewModel> allWarehouses, List<PermissionTypeAllNodeViewModel> allPermissionTypes)
        {
            AllWarehouses = allWarehouses;
            AllPermissionTypes = allPermissionTypes;
            foreach (var warehouse in AllWarehouses)
                warehouse.ItemChangeSelectAll += IsAllWarehouseSeted;
            foreach (var permission in AllPermissionTypes)
                permission.ItemChangeSelectAll += IsAllPermissionSeted;
        }
        private List<WarehouseAllNodeViewModel> allWarehouses;

        public List<WarehouseAllNodeViewModel> AllWarehouses
        {
            get => allWarehouses;
            set => SetField(ref allWarehouses, value);
        }

        private List<PermissionTypeAllNodeViewModel> allPermissionTypes;

        public List<PermissionTypeAllNodeViewModel> AllPermissionTypes
        {
            get => allPermissionTypes;
            set => SetField(ref allPermissionTypes, value);
        }

        private bool? permissionValue;

        public bool? PermissionValue
        {
            get => permissionValue;
            set
            {
                if (UnSetAll) SetField(ref permissionValue, value);
                else if (SetField(ref permissionValue, value))
                {
                    foreach (var permissionTypeAll in AllPermissionTypes)
                    {
                        permissionTypeAll.UnSubscribeAll = true;
                        permissionTypeAll.PermissionValue = value;
                        permissionTypeAll.UnSubscribeAll = false;
                    }
                    foreach (var warehouseAll in AllWarehouses)
                    {
                        warehouseAll.UnSubscribeAll = true;
                        warehouseAll.PermissionValue = value;
                        warehouseAll.UnSubscribeAll = false;
                    }
                }
            }
            
        }

        private void IsAllPermissionSeted(object sender, EventArgs e)
        {
            var permissionTypeAllNodeViewModel = sender as PermissionTypeAllNodeViewModel;
            if (permissionTypeAllNodeViewModel.UnSubscribeAll) return;
            UnSetAll = true;
            if (AllPermissionTypes.All(x => x.PermissionValue == true) 
                    && permissionTypeAllNodeViewModel.PermissionValue == true)
                PermissionValue = permissionTypeAllNodeViewModel.PermissionValue;
            else if (AllPermissionTypes.All(x => x.PermissionValue == false)
                     && permissionTypeAllNodeViewModel.PermissionValue == false)
                PermissionValue = permissionTypeAllNodeViewModel.PermissionValue;
            else PermissionValue = null;
            UnSetAll = false;
        }

        private void IsAllWarehouseSeted(object sender, EventArgs e)
        {
            var warehouseAllNodeViewModel = sender as WarehouseAllNodeViewModel;
            if (warehouseAllNodeViewModel.UnSubscribeAll) return;
            UnSetAll = true;
            if (AllWarehouses.All(x => x.PermissionValue == true) 
                && warehouseAllNodeViewModel.PermissionValue == true)
                PermissionValue = warehouseAllNodeViewModel.PermissionValue;
            else if (AllWarehouses.All(x => x.PermissionValue == false)
                     && warehouseAllNodeViewModel.PermissionValue == false)
                PermissionValue = warehouseAllNodeViewModel.PermissionValue;
            else PermissionValue = null;
            UnSetAll = false;
        }

        public bool UnSetAll = false;
    }
}