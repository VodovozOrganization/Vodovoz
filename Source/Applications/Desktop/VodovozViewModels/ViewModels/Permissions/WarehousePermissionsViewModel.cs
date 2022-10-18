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
		private readonly IList<Warehouse> _allWarehouses;

		private bool _canEdit;
		private SelectAllNodePermissionViewModel _allPermissions;
		private List<PermissionTypeAllNodeViewModel> _allPermissionTypes;
		private List<WarehouseAllNodeViewModel> _warehousesAllNodesViewModels;
		private WarehousePermissionModelBase _warehousePermissionModelBase;
		
		private IEnumerable<WarehousePermissionsType> AllPermissionsTypes() =>
			Enum.GetValues(typeof(WarehousePermissionsType)).Cast<WarehousePermissionsType>();

		public WarehousePermissionsViewModel(IUnitOfWork uow, WarehousePermissionModelBase warehousePermissionModelBase)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			
			_allWarehouses = uow.Session.QueryOver<Warehouse>().List();
			WarehousePermissionModelBase = warehousePermissionModelBase ?? throw new ArgumentNullException(nameof(warehousePermissionModelBase));
			AllPermissionTypes = new List<PermissionTypeAllNodeViewModel>();
			AllWarehouses = new List<WarehouseAllNodeViewModel>();

			CreateNodesViewModels();
		}

		public SelectAllNodePermissionViewModel AllPermissions
		{
			get => _allPermissions;
			set => SetField(ref _allPermissions, value);
		}

		public WarehousePermissionModelBase WarehousePermissionModelBase
		{
			get => _warehousePermissionModelBase;
			set => SetField(ref _warehousePermissionModelBase, value);
		}

		public List<WarehouseAllNodeViewModel> AllWarehouses
		{
			get => _warehousesAllNodesViewModels;
			set => SetField(ref _warehousesAllNodesViewModels, value);
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

		public void UpdateData(IList<WarehousePermissionBase> newUserPermissions)
		{
			WarehousePermissionModelBase.AllPermission = newUserPermissions;
			CreateNodesViewModels();
		}

		public void SaveWarehousePermissions()
		{
			foreach(var warehouse in AllWarehouses.SelectMany(allWarehouse => allWarehouse.SubNodeViewModel))
			{
				if(warehouse.PermissionValue is null)
				{
					WarehousePermissionModelBase.DeletePermission(warehouse.WarehousePermissionsType, warehouse.Warehouse);
				}
				else
				{
					WarehousePermissionModelBase.AddOnUpdatePermission(warehouse.WarehousePermissionsType, warehouse.Warehouse,
						warehouse.PermissionValue);
				}
			}
		}
		
		private void CreateNodesViewModels()
		{
			AllPermissionTypes.Clear();
			AllWarehouses.Clear();
			
			foreach(var permissionsType in AllPermissionsTypes())
			{
				var permissionAllNode =
					new PermissionTypeAllNodeViewModel(permissionsType, _allWarehouses, _warehousePermissionModelBase);
				AllPermissionTypes.Add(permissionAllNode);
			}

			foreach(var warehouse in _allWarehouses)
			{
				var warehouseAllNode = new WarehouseAllNodeViewModel(warehouse, AllPermissionsTypes(), _warehousePermissionModelBase);
				AllWarehouses.Add(warehouseAllNode);
			}

			AllPermissions = new SelectAllNodePermissionViewModel(AllWarehouses, AllPermissionTypes) { Title = "Все" };
		}
	}
}
