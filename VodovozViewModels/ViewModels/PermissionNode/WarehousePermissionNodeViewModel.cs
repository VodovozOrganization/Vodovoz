using System;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehousePermissionNodeViewModel : PropertyChangedBase
    {
        private WarehousePermissionModel warehousePermissionModel { get; set; }

        public WarehousePermissionNodeViewModel(Warehouse warehouse, WarehousePermissions warehousePermissions, WarehousePermissionModel warehousePermissionModel)
        {
            this.warehouse = warehouse;
            this.warehousePermissions = warehousePermissions;
            this.warehousePermissionModel = warehousePermissionModel;
            var permissions = warehousePermissionModel.AllPermission;
            if (permissions.Any())
            {
                this.permissionValue = permissions
                    .Where(x => x.Warehouse == Warehouse && x.WarehousePermissionType == WarehousePermissions)
                    .Select(x=>x.ValuePermission).SingleOrDefault();
            }
            UnSubscribe = false;
        }

        private WarehousePermissions warehousePermissions;
        public WarehousePermissions WarehousePermissions
        {
            get => warehousePermissions;
            set => SetField(ref warehousePermissions, value);
        }

        private Warehouse warehouse;
        public Warehouse Warehouse
        {
            get => warehouse;
            set => SetField(ref warehouse, value);
        }

        private bool? permissionValue;
        public bool? PermissionValue
        {
            get => permissionValue;
            set
            {
                SetField(ref permissionValue, value);
                ItemChangeValue?.Invoke(this, EventArgs.Empty);
            }
        }

        public EventHandler ItemChangeValue;

        private bool unSubscribe;
        public bool UnSubscribe
        {
            get => unSubscribe;
            set => SetField(ref unSubscribe, value);
        }
    }
}