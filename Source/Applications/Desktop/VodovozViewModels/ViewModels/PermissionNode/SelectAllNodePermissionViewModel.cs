using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.PermissionNode
{
	public class SelectAllNodePermissionViewModel : PropertyChangedBase, IPermissionNodeViewModel
	{
		private string _title;
		public string Title
		{
			get => _title; 
			set => SetField(ref _title, value);
		}
		
		public SelectAllNodePermissionViewModel(List<WarehouseAllNodeViewModel> allWarehouses, List<PermissionTypeAllNodeViewModel> allPermissionTypes)
		{
			AllWarehouses = allWarehouses;
			AllPermissionTypes = allPermissionTypes;
			AllWarehouses.ForEach(x => x.ItemSelectAllChanged += InstallAllWarehouses);
			AllPermissionTypes.ForEach(x => x.ItemSelectAllChanged += InstallAllPermissions);
		}
		private List<WarehouseAllNodeViewModel> _allWarehouses;

		public List<WarehouseAllNodeViewModel> AllWarehouses
		{
			get => _allWarehouses;
			set => SetField(ref _allWarehouses, value);
		}

		private List<PermissionTypeAllNodeViewModel> _allPermissionTypes;

		public List<PermissionTypeAllNodeViewModel> AllPermissionTypes
		{
			get => _allPermissionTypes;
			set => SetField(ref _allPermissionTypes, value);
		}

		private bool? _permissionValue;

		public bool? PermissionValue
		{
			get => _permissionValue;
			set
			{
				if(UnSetAll)
				{
					SetField(ref _permissionValue, value);
				}
				else if(SetField(ref _permissionValue, value))
				{
					foreach(var permissionTypeAll in AllPermissionTypes)
					{
						permissionTypeAll.UnsubscribedAll = true;
						permissionTypeAll.PermissionValue = value;
						permissionTypeAll.UnsubscribedAll = false;
					}
					foreach(var warehouseAll in AllWarehouses)
					{
						warehouseAll.UnsubscribedAll = true;
						warehouseAll.PermissionValue = value;
						warehouseAll.UnsubscribedAll = false;
					}
				}
			}
			
		}

		private void InstallAllPermissions(object sender, EventArgs e)
		{
			var permissionTypeAllNodeViewModel = sender as PermissionTypeAllNodeViewModel;
			if(permissionTypeAllNodeViewModel.UnsubscribedAll)
			{
				return;
			}

			UnSetAll = true;
			if(AllPermissionTypes.All(x => x.PermissionValue == true) 
					&& permissionTypeAllNodeViewModel.PermissionValue == true)
			{
				PermissionValue = permissionTypeAllNodeViewModel.PermissionValue;
			}
			else if(AllPermissionTypes.All(x => x.PermissionValue == false)
					&& permissionTypeAllNodeViewModel.PermissionValue == false)
			{
				PermissionValue = permissionTypeAllNodeViewModel.PermissionValue;
			}
			else
			{
				PermissionValue = null;
			}

			UnSetAll = false;
		}

		private void InstallAllWarehouses(object sender, EventArgs e)
		{
			var warehouseAllNodeViewModel = sender as WarehouseAllNodeViewModel;
			if(warehouseAllNodeViewModel.UnsubscribedAll)
			{
				return;
			}

			UnSetAll = true;
			if(AllWarehouses.All(x => x.PermissionValue == true) 
				&& warehouseAllNodeViewModel.PermissionValue == true)
			{
				PermissionValue = warehouseAllNodeViewModel.PermissionValue;
			}
			else if(AllWarehouses.All(x => x.PermissionValue == false)
					&& warehouseAllNodeViewModel.PermissionValue == false)
			{
				PermissionValue = warehouseAllNodeViewModel.PermissionValue;
			}
			else
			{
				PermissionValue = null;
			}

			UnSetAll = false;
		}

		public bool UnSetAll = false;
	}
}
