using System;
using Gtk;
using QS.Dialog.Gtk;
using QS.Project.Services;
using QS.Services;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.Representations;

namespace Vodovoz.JournalViewers
{
	public partial class WarehousesView : TdiTabBase
	{
		private readonly IPermissionResult _permissionResult =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Warehouse));
		private WarehousesVM _vm;
		
		public WarehousesView()
		{
			Build();
			TabName = "Журнал складов";
			Configure();
		}

		private void Configure()
		{
			_vm = new WarehousesVM();
			tableWarehouses.ColumnsConfig = _vm.ColumnsConfig;
			tableWarehouses.Selection.Changed += OnSelectionChanged;
			btnAdd.Sensitive = _permissionResult.CanCreate;

			Update();
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			bool isSensitive = tableWarehouses.Selection.CountSelectedRows() > 0
				&& (tableWarehouses.GetSelectedObjects()[0] as SubdivisionWithWarehousesVMNode).WarehouseId.HasValue;

			buttonEdit.Sensitive = isSensitive && _permissionResult.CanRead;
			buttonDelete.Sensitive = isSensitive && _permissionResult.CanDelete;
		}

		private void Update()
		{
			_vm.UpdateNodes();
			tableWarehouses.YTreeModel = _vm.TreeModel;
			tableWarehouses.ExpandAll();
		}
		
		private void DisposeUowAndUpdate()
		{
			_vm?.UoW?.Dispose();
			Update();
		}

		private WarehouseDlg CreateWarehouseDlg(int warehouseId)
		{
			var dlg = warehouseId == 0 ? new WarehouseDlg() : new WarehouseDlg(warehouseId);
			dlg.EntitySaved += (o, args) => DisposeUowAndUpdate();
			return dlg;
		}

		protected void OnBtnAddClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Warehouse>(0),
				() => CreateWarehouseDlg(0),
				this
			);
		}

		protected void OnTableWarehousesRowActivated(object o, RowActivatedArgs args)
		{
			if(_permissionResult.CanRead)
			{
				buttonEdit.Click();
			}
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if(tableWarehouses.GetSelectedObjects().GetLength(0) > 0) {
				int? id = (tableWarehouses.GetSelectedObjects()[0] as SubdivisionWithWarehousesVMNode).WarehouseId;
				if(id.HasValue)
					TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<Warehouse>(id.Value),
							() => CreateWarehouseDlg(id.Value),
							this
						);
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var item = tableWarehouses.GetSelectedObject<SubdivisionWithWarehousesVMNode>();
			if(item.WarehouseId.HasValue && OrmMain.DeleteObject<Warehouse>(item.WarehouseId.Value))
			{
				DisposeUowAndUpdate();
			}
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e) => DisposeUowAndUpdate();

		protected override void OnDestroyed()
		{
			_vm?.Destroy();
			base.OnDestroyed();
		}
	}
}
