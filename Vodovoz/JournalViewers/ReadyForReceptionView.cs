using System;
using QS.DomainModel.UoW;
using QS.Dialog;
using Vodovoz.ViewModel;
using QS.Widgets.GtkUI;
using QS.Project.Journal.EntitySelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Domain.Store;
using QS.Project.Services;

namespace Vodovoz
{
	public partial class ReadyForReceptionView : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
    {
		private IUnitOfWork uow;

		ReadyForReceptionVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
			}
		}

		public ReadyForReceptionView()
		{
			this.Build ();
			this.TabName = "Готовые к разгрузке";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
            viewModel = new ReadyForReceptionVM(UoW);

            if (viewModel.Filter.WarehousesAmount > 5)
            {
                var entryWarehouses = new EntityViewModelEntry();
                entryWarehouses.SetEntityAutocompleteSelectorFactory(
                    new EntityAutocompleteSelectorFactory<WarehouseJournalViewModel>(typeof(Warehouse),
                        () => new WarehouseJournalViewModel(viewModel.Filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices))
                );
                entryWarehouses.Binding.AddBinding(viewModel.Filter, vm => vm.RestrictWarehouse, w => w.Subject).InitializeFromSource();
                entryWarehouses.ChangedByUser += (sender, e) => tableReadyForReception.RepresentationModel.UpdateNodes();
                entryWarehouses.CompletionPopupSetWidth(false);
                hbox3.Add(entryWarehouses);
            }
            else
            {
                var comboWarehouses = new SpecialListComboBox();
                comboWarehouses.SetRenderTextFunc((Warehouse w) => w.Name);
                comboWarehouses.ItemsList = viewModel.Filter.Warehouses;
                comboWarehouses.Binding.AddBinding(viewModel.Filter, vm => vm.RestrictWarehouse, w => w.SelectedItem);
                comboWarehouses.Changed += (sender, e) => tableReadyForReception.RepresentationModel.UpdateNodes();
                hbox3.Add(comboWarehouses);
            }
            hbox3.ShowAll();

            checkWithoutUnload.Binding.AddBinding(viewModel.Filter, vm => vm.RestrictWithoutUnload, w => w.Active);

            tableReadyForReception.RepresentationModel = viewModel;
            tableReadyForReception.RepresentationModel.UpdateNodes();
            tableReadyForReception.Selection.Changed += OnSelectionChanged;
            checkWithoutUnload.Toggled += (o, args) => tableReadyForReception.RepresentationModel.UpdateNodes();
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonOpen.Sensitive = tableReadyForReception.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			var node = tableReadyForReception.GetSelectedNode () as ReadyForReceptionVMNode;
			var dlg = new CarUnloadDocumentDlg (node.Id, viewModel.Filter.RestrictWarehouse?.Id);
			TabParent.AddTab (dlg, this);
		}

		protected void OnTableReadyForReceptionRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonOpen.Click ();
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableReadyForReception.SearchHighlightText = searchentity1.Text;
			tableReadyForReception.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableReadyForReception.RepresentationModel.UpdateNodes();
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

