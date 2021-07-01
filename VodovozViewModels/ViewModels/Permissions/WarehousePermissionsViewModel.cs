using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.ViewModels.PermissionNode;

namespace Vodovoz.ViewModels.Permissions
{
	public class WarehousePermissionsViewModel : UoWWidgetViewModelBase
	{
		private IUnitOfWork _uow;
		private bool _canEdit;
		private SelectAllNodePermissionViewModel _allPermissions;
		private List<PermissionTypeAllNodeViewModel> _allPermissionTypes;
		private WarehouseAllNodeViewModel _warehouseAllNode;
		private PermissionTypeAllNodeViewModel _permissionAllNode;
		private List<WarehouseAllNodeViewModel> _allWarehouses;
		private WarehousePermissionModel _subdivisionWarehousePermissionModel;
		private IEnumerable<WarehousePermissions> AllPermissionsTypes() => Enum.GetValues(typeof(WarehousePermissions)).Cast<WarehousePermissions>();
		private IEnumerable<Warehouse> allNamesOfWarehouses() => _uow.Session.QueryOver<Warehouse>().List();

		public WarehousePermissionsViewModel(IUnitOfWork UoW, WarehousePermissionModel warehousePermissionModel)
		{
			this._uow = UoW;
			WarehousePermissionModel = warehousePermissionModel ?? throw new ArgumentNullException(nameof(warehousePermissionModel));
			AllPermissionTypes = new List<PermissionTypeAllNodeViewModel>();
			AllWarehouses = new List<WarehouseAllNodeViewModel>();

			foreach(var permissionsType in AllPermissionsTypes())
			{
				_permissionAllNode = new PermissionTypeAllNodeViewModel(permissionsType, allNamesOfWarehouses(), _subdivisionWarehousePermissionModel);
				AllPermissionTypes.Add(_permissionAllNode);
			}
			foreach(var warehouse in allNamesOfWarehouses())
			{
				_warehouseAllNode = new WarehouseAllNodeViewModel(warehouse, AllPermissionsTypes(), _subdivisionWarehousePermissionModel);
				AllWarehouses.Add(_warehouseAllNode);
			}

			AllPermissions = new SelectAllNodePermissionViewModel(AllWarehouses, AllPermissionTypes) { Title = "Все" };
		}

		public SelectAllNodePermissionViewModel AllPermissions
		{
			get => _allPermissions;
			set => SetField(ref _allPermissions, value);
		}

		public WarehousePermissionModel WarehousePermissionModel
		{
			get => _subdivisionWarehousePermissionModel;
			set => SetField(ref _subdivisionWarehousePermissionModel, value);
		}

		public List<WarehouseAllNodeViewModel> AllWarehouses
		{
			get => _allWarehouses;
			set => SetField(ref _allWarehouses, value);
		}

		public List<PermissionTypeAllNodeViewModel> AllPermissionTypes
		{
			get => _allPermissionTypes;
			set => SetField(ref _allPermissionTypes, value);
		}

		public bool CanEdit
		{
			get => _canEdit;
			set => SetField(ref _canEdit, value);
		}

		public void SaveWarehousePermissions()
		{
			foreach(var allWarehouse in AllWarehouses)
			{
				foreach(var warehouse in allWarehouse.SubNodeViewModel)
				{
					if(warehouse.PermissionValue is null)
						WarehousePermissionModel.DeletePermission(warehouse.WarehousePermissions,
							warehouse.Warehouse);
					else
						WarehousePermissionModel.AddOnUpdatePermission(warehouse.WarehousePermissions,
							warehouse.Warehouse, warehouse.PermissionValue);
				}
			}
		}
	}
}