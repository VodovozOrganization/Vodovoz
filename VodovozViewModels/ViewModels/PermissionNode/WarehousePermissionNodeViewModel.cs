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
            this._warehouse = warehouse;
            this._warehousePermissionsType = warehousePermissionsType;
            this.WarehousePermissionModelBase = warehousePermissionModelBase;
            var permissions = warehousePermissionModelBase.AllPermission;
            if (permissions.Any())
            {
                this._permissionValue = permissions
                    .Where(x => x.Warehouse == Warehouse && x.WarehousePermissionType == WarehousePermissionsType)
                    .Select(x=>x.PermissionValue).SingleOrDefault();
            }
            Unsubscribed = false;
        }

        private WarehousePermissionsType _warehousePermissionsType;
        public WarehousePermissionsType WarehousePermissionsType
        {
            get => _warehousePermissionsType;
            set => SetField(ref _warehousePermissionsType, value);
        }

        private Warehouse _warehouse;
        public Warehouse Warehouse
        {
            get => _warehouse;
            set => SetField(ref _warehouse, value);
        }

        private bool? _permissionValue;
        public bool? PermissionValue
        {
            get => _permissionValue;
            set
            {
                if(SetField(ref _permissionValue, value))
	                ItemValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public EventHandler ItemValueChanged;

        private bool _unsubscribed;
        public bool Unsubscribed
        {
            get => _unsubscribed;
            set => SetField(ref _unsubscribed, value);
        }
    }
}
