using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Permissions.Warehouses;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
	public class WarehouseAllNodeViewModel : WarehousePermissionAllNodeViewModelBase
	{
		private Warehouse _warehouse;

		public Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		public WarehouseAllNodeViewModel(Warehouse warehouse, IEnumerable<WarehousePermissionsType> permissionTypes,
			WarehousePermissionModelBase warehousePermissionModelBase)
		{
			_warehouse = warehouse;
			Title = warehouse.Name;

			SubNodeViewModel = new GenericObservableList<WarehousePermissionNodeViewModel>();
			foreach(var permission in permissionTypes)
			{
				var warehouseNode = new WarehousePermissionNodeViewModel(Warehouse, permission, warehousePermissionModelBase);
				warehouseNode.ItemValueChanged += InstallAllWarehouses;
				SubNodeViewModel.Add(warehouseNode);
				InstallAllWarehouses(warehouseNode, EventArgs.Empty);
			}
		}

		public void InstallAllWarehouses(object sender, EventArgs e)
		{
			var warehousePermissionNodeViewModel = sender as WarehousePermissionNodeViewModel;
			if(warehousePermissionNodeViewModel.Unsubscribed)
			{
				return;
			}
			UnSetAll = true;
			var collection = SubNodeViewModel.Where(x => x.WarehousePermissionsType !=
														 warehousePermissionNodeViewModel.WarehousePermissionsType);
			if(collection.All(x => x.PermissionValue == true) && warehousePermissionNodeViewModel.PermissionValue == true)
			{
				PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
			}
			else if(collection.All(x => x.PermissionValue == false) && warehousePermissionNodeViewModel.PermissionValue == false)
			{
				PermissionValue = warehousePermissionNodeViewModel.PermissionValue;
			}
			else
			{
				PermissionValue = null;
			}

			UnSetAll = false;
		}
	}
}
