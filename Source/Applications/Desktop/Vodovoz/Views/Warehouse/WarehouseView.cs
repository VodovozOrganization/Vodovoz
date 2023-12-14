using QS.Views.GtkUI;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
    public partial class WarehouseView : TabViewBase<WarehouseViewModel>
    {
        public WarehouseView(WarehouseViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureView();
        }

        private void ConfigureView()
        {
            entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
            checkOnlineStore.Binding.AddBinding(ViewModel.Entity, e => e.PublishOnlineStore, w => w.Active).InitializeFromSource();
            checkCanReceiveBottles.Binding.AddBinding(ViewModel.Entity, e => e.CanReceiveBottles, w => w.Active).InitializeFromSource();
            checkCanReceiveBottles.Binding.AddBinding(ViewModel.Entity, e => e.CanReceiveEquipment, w => w.Active).InitializeFromSource();
            checkArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			checkArchive.Sensitive = ViewModel.CanArchiveWarehouse;

			comboEnumTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
            comboEnumTypeOfUse.Binding.AddBinding(ViewModel.Entity, e => e.TypeOfUse, w => w.SelectedItem).InitializeFromSource();

            comboOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
            comboOwner.ItemsList = ViewModel.Subdivisions;
            comboOwner.Binding.AddBinding(ViewModel.Entity, e => e.OwningSubdivision, w => w.SelectedItem).InitializeFromSource();

			entryAddress.Binding.AddBinding(ViewModel.Entity, e => e.Address, w => w.Text).InitializeFromSource();

			btnSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
            btnCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
        }
    }
}
