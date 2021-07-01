using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.ViewModels.PermissionNode;

namespace Vodovoz.ViewModels.Permissions
{
    public class WarehousePermissionsViewModel : UoWWidgetViewModelBase
    {
        private IUnitOfWork UoW;
        private IPermissionResult permissionResult;

        public WarehousePermissionsViewModel(IUnitOfWork UoW, IPermissionResult permissionResult, Subdivision subdivision)
        {
            this.UoW = UoW;
            this.permissionResult = permissionResult;
            userWarehousePermissionModel = new UserWarehousePermissionModel();
            SubdivisionWarehousePermissionModel = new SubdivisionWarehousePermissionModel(UoW, subdivision);
            AllPermissionTypes = new List<PermissionTypeAllNodeViewModel>();
            AllWarehouses = new List<WarehouseAllNodeViewModel>();
            
            foreach (var permissionsType in AllPermissionsTypes())
            {
                permissionAllNode = new PermissionTypeAllNodeViewModel(permissionsType, allNamesOfWarehouses(), subdivisionWarehousePermissionModel);
                AllPermissionTypes.Add(permissionAllNode);
            }
            foreach (var warehouse in allNamesOfWarehouses())
            {
                warehouseAllNode = new WarehouseAllNodeViewModel(warehouse, AllPermissionsTypes(), subdivisionWarehousePermissionModel);
                AllWarehouses.Add(warehouseAllNode);
            }
            
            AllPermissions = new SelectAllNodePermissionViewModel(AllWarehouses, AllPermissionTypes){ Title = "Все"};
        }

        private SelectAllNodePermissionViewModel allPermissions;

        public SelectAllNodePermissionViewModel AllPermissions
        {
            get => allPermissions;
            set => SetField(ref allPermissions, value);
        }
        
        private WarehousePermissionModel userWarehousePermissionModel { get; set; }

        private WarehousePermissionModel subdivisionWarehousePermissionModel;

        public WarehousePermissionModel SubdivisionWarehousePermissionModel
        {
            get => subdivisionWarehousePermissionModel;
            set => SetField(ref subdivisionWarehousePermissionModel, value);
        }

        private WarehouseAllNodeViewModel warehouseAllNode;
        private PermissionTypeAllNodeViewModel permissionAllNode;

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

        public bool CanEdit => permissionResult.CanUpdate;

        public void SaveWarehousePermissions()
        {
            foreach (var allWarehouse in AllWarehouses)
            {
                foreach (var warehouse in allWarehouse.SubNodeViewModel)
                {
                    if (warehouse.PermissionValue is null)
                        SubdivisionWarehousePermissionModel.DeletePermission(warehouse.WarehousePermissions,
                            warehouse.Warehouse);
                    else
                        SubdivisionWarehousePermissionModel.AddOnUpdatePermission(warehouse.WarehousePermissions,
                            warehouse.Warehouse, warehouse.PermissionValue);
                }
            }
        }
        
        private IEnumerable<WarehousePermissions> AllPermissionsTypes() => Enum.GetValues(typeof(WarehousePermissions)).Cast<WarehousePermissions>();

        private IEnumerable<Warehouse> allNamesOfWarehouses() => UoW.Session.QueryOver<Warehouse>().List();
    }
}