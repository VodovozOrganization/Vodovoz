using System;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
    public class WarehousePermissionNodeViewModel : PropertyChangedBase
    {
        private WarehousePermissionModelBase WarehousePermissionModelBase { get; set; }

        public WarehousePermissionNodeViewModel(Warehouse warehouse, WarehousePermissionsType warehousePermissionsType, WarehousePermissionModelBase warehousePermissionModelBase)
        {
            this.warehouse = warehouse;
            this._warehousePermissionsType = warehousePermissionsType;
            this.WarehousePermissionModelBase = warehousePermissionModelBase;
            var permissions = warehousePermissionModelBase.AllPermission;
            if (permissions.Any())
            {
                this.permissionValue = permissions
                    .Where(x => x.Warehouse == Warehouse && x.WarehousePermissionTypeType == WarehousePermissionsType)
                    .Select(x=>x.PermissionValue).SingleOrDefault();
            }
            UnSubscribe = false;
        }

        private WarehousePermissionsType _warehousePermissionsType;
        public WarehousePermissionsType WarehousePermissionsType
        {
            get => _warehousePermissionsType;
            set => SetField(ref _warehousePermissionsType, value);
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