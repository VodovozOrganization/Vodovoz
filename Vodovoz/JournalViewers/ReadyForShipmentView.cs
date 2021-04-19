using System;
using Gamma.Widgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Widgets.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;

namespace Vodovoz
{
    public partial class ReadyForShipmentView : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
    {
        private IUnitOfWork uow;

        ReadyForShipmentVM viewModel;

        public IUnitOfWork UoW
        {
            get => uow;
            set
            {
                if (uow == value)
                    return;
                uow = value;
            }
        }

        public ReadyForShipmentView()
        {
            this.Build();
            this.TabName = "Готовые к погрузке";
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            viewModel = new ReadyForShipmentVM(UoW);

            if (viewModel.Filter.WarehousesAmount > 5)
            {
                var entryWarehouses = new EntityViewModelEntry();
                entryWarehouses.SetEntityAutocompleteSelectorFactory(
                    new EntityAutocompleteSelectorFactory<WarehouseJournalViewModel>(typeof(Warehouse),
                        () => new WarehouseJournalViewModel(viewModel.Filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices))
                );
                entryWarehouses.Binding.AddBinding(viewModel.Filter, vm => vm.RestrictWarehouse, w => w.Subject).InitializeFromSource();
                entryWarehouses.ChangedByUser += (sender, e) => tableReadyForShipment.RepresentationModel.UpdateNodes();
                entryWarehouses.CompletionPopupSetWidth(false);
                hbox3.Add(entryWarehouses);
            }
            else
            {
                var comboWarehouses = new SpecialListComboBox();
                comboWarehouses.SetRenderTextFunc((Warehouse w) => w.Name);
                comboWarehouses.ItemsList = viewModel.Filter.Warehouses;
                comboWarehouses.Binding.AddBinding(viewModel.Filter, vm => vm.RestrictWarehouse, w => w.SelectedItem);
                comboWarehouses.Changed += (sender, e) => tableReadyForShipment.RepresentationModel.UpdateNodes();
                hbox3.Add(comboWarehouses);
            }
            hbox3.ShowAll();

            tableReadyForShipment.RepresentationModel = viewModel;
            tableReadyForShipment.RepresentationModel.UpdateNodes();
            tableReadyForShipment.Selection.Changed += OnSelectionChanged;
        }

        RepresentationSelectResult[] lastMenuSelected;

        void OnSelectionChanged(object sender, EventArgs e)
        {
            buttonOpen.Sensitive = tableReadyForShipment.Selection.CountSelectedRows() > 0;

            if (tableReadyForShipment.GetSelectedObject() is ReadyForShipmentVMNode selectedNode)
                lastMenuSelected = new[] { new RepresentationSelectResult(selectedNode.Id, selectedNode) };
        }

        protected void OnButtonOpenClicked(object sender, EventArgs e)
        {
            var node = tableReadyForShipment.GetSelectedNode() as ReadyForShipmentVMNode;
            var dlg = new CarLoadDocumentDlg(node.Id, viewModel.Filter.RestrictWarehouse?.Id);
            TabParent.AddTab(dlg, this);
        }

        protected void OnTableReadyForShipmentRowActivated(object o, Gtk.RowActivatedArgs args)
        {
            buttonOpen.Click();
        }

        protected void OnSearchentity2TextChanged(object sender, EventArgs e)
        {
            tableReadyForShipment.SearchHighlightText = searchentity2.Text;
            tableReadyForShipment.RepresentationModel.SearchString = searchentity2.Text;
        }

        protected void OnButtonRefreshClicked(object sender, EventArgs e)
        {
            tableReadyForShipment.RepresentationModel.UpdateNodes();
        }

        public override void Destroy()
        {
            UoW?.Dispose();
            base.Destroy();
        }
    }
}

