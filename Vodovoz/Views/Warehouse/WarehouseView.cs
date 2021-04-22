using QS.Project.Services;
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
            checkArchive.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse");

            comboEnumTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
            comboEnumTypeOfUse.Binding.AddBinding(ViewModel.Entity, e => e.TypeOfUse, w => w.SelectedItem).InitializeFromSource();

            comboOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
            comboOwner.ItemsList = new EntityRepositories.Subdivisions.SubdivisionRepository().GetAllDepartments(ViewModel.UoW);
            comboOwner.Binding.AddBinding(ViewModel.Entity, e => e.OwningSubdivision, w => w.SelectedItem).InitializeFromSource();

            btnSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
            btnCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
        }
    }
}
