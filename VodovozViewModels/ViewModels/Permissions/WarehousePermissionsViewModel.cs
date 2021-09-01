using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Permissions.Warehouses;
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
		private WarehousePermissionModelBase _subdivisionWarehousePermissionModelBase;
		private IEnumerable<WarehousePermissionsType> AllPermissionsTypes() => Enum.GetValues(typeof(WarehousePermissionsType)).Cast<WarehousePermissionsType>();
		private IEnumerable<Warehouse> allNamesOfWarehouses() => _uow.Session.QueryOver<Warehouse>().List();

		public WarehousePermissionsViewModel(IUnitOfWork UoW, WarehousePermissionModelBase warehousePermissionModelBase)
		{
			this._uow = UoW;
			WarehousePermissionModelBase = warehousePermissionModelBase ?? throw new ArgumentNullException(nameof(warehousePermissionModelBase));
			AllPermissionTypes = new List<PermissionTypeAllNodeViewModel>();
			AllWarehouses = new List<WarehouseAllNodeViewModel>();

			foreach(var permissionsType in AllPermissionsTypes())
			{
				_permissionAllNode = new PermissionTypeAllNodeViewModel(permissionsType, allNamesOfWarehouses(), _subdivisionWarehousePermissionModelBase);
				AllPermissionTypes.Add(_permissionAllNode);
			}
			foreach(var warehouse in allNamesOfWarehouses())
			{
				_warehouseAllNode = new WarehouseAllNodeViewModel(warehouse, AllPermissionsTypes(), _subdivisionWarehousePermissionModelBase);
				AllWarehouses.Add(_warehouseAllNode);
			}

			AllPermissions = new SelectAllNodePermissionViewModel(AllWarehouses, AllPermissionTypes) { Title = "Все" };
		}

		public SelectAllNodePermissionViewModel AllPermissions
		{
			get => _allPermissions;
			set => SetField(ref _allPermissions, value);
		}

		public WarehousePermissionModelBase WarehousePermissionModelBase
		{
			get => _subdivisionWarehousePermissionModelBase;
			set => SetField(ref _subdivisionWarehousePermissionModelBase, value);
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
						WarehousePermissionModelBase.DeletePermission(warehouse.WarehousePermissionsType,
							warehouse.Warehouse);
					else
						WarehousePermissionModelBase.AddOnUpdatePermission(warehouse.WarehousePermissionsType,
							warehouse.Warehouse, warehouse.PermissionValue);
				}
			}
		}
	}
}