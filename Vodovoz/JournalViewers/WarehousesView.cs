using System;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.Representations;

namespace Vodovoz.JournalViewers
{
	public partial class WarehousesView : QS.Dialog.Gtk.TdiTabBase
	{
		IUnitOfWork uow;

		public WarehousesView()
		{
			this.Build();
			this.TabName = "Журнал складов";
			ConfigureWidget();
		}

		void ConfigureWidget()
		{
			var vm = new WarehousesVM();
			tableWarehouses.ColumnsConfig = vm.ColumnsConfig;
			vm.UpdateNodes();
			tableWarehouses.YTreeModel = vm.TreeModel;
			uow = vm.UoW;
			tableWarehouses.Selection.Changed += OnSelectionChanged;
			tableWarehouses.ExpandAll();
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool isSensitive = tableWarehouses.Selection.CountSelectedRows() > 0
				&& (tableWarehouses.GetSelectedObjects()[0] as SubdivisionWithWarehousesVMNode).WarehouseId.HasValue;

			buttonEdit.Sensitive = isSensitive;
			buttonDelete.Sensitive = isSensitive;
		}

		protected void OnBtnAddClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Warehouse>(0),
				() => new WarehouseDlg(),
				this
			);
		}

		protected void OnTableWarehousesRowActivated(object o, RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if(tableWarehouses.GetSelectedObjects().GetLength(0) > 0) {
				int? id = (tableWarehouses.GetSelectedObjects()[0] as SubdivisionWithWarehousesVMNode).WarehouseId;
				if(id.HasValue)
					TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<Warehouse>(id.Value),
							() => {
								var dlg = new WarehouseDlg(id.Value);
								dlg.EntitySaved += (s, ea) => ConfigureWidget();
								return dlg;
							},
							this
						);
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var item = tableWarehouses.GetSelectedObject<SubdivisionWithWarehousesVMNode>();
			if(item.WarehouseId.HasValue && OrmMain.DeleteObject<Warehouse>(item.WarehouseId.Value))
				tableWarehouses.RepresentationModel.UpdateNodes();
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableWarehouses.SearchHighlightText = searchentity1.Text;
			tableWarehouses.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e) => ConfigureWidget();
	}
}
