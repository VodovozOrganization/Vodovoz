using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class WarehouseView : TabViewBase<WarehouseViewModel>
	{
		public WarehouseView(WarehouseViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			entryName.IsEditable = ViewModel.CanEdit;
			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			
			checkOnlineStore.Sensitive = ViewModel.CanEdit;
			checkOnlineStore.Binding
				.AddBinding(ViewModel.Entity, e => e.PublishOnlineStore, w => w.Active)
				.InitializeFromSource();
			
			checkCanReceiveEquipment.Sensitive = ViewModel.CanEdit;
			checkCanReceiveEquipment.Binding
				.AddBinding(ViewModel.Entity, e => e.CanReceiveEquipment, w => w.Active)
				.InitializeFromSource();
			
			checkCanReceiveBottles.Sensitive = ViewModel.CanEdit;
			checkCanReceiveBottles.Binding
				.AddBinding(ViewModel.Entity, e => e.CanReceiveBottles, w => w.Active)
				.InitializeFromSource();
			
			checkArchive.Sensitive = ViewModel.CanArchiveWarehouse;
			checkArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			comboEnumTypeOfUse.Sensitive = ViewModel.CanEdit;
			comboEnumTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
			comboEnumTypeOfUse.Binding
				.AddBinding(ViewModel.Entity, e => e.TypeOfUse, w => w.SelectedItem)
				.InitializeFromSource();

			comboOwner.Sensitive = ViewModel.CanEdit;
			comboOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboOwner.ItemsList = ViewModel.Subdivisions;
			comboOwner.Binding
				.AddBinding(ViewModel, vm => vm.OwningSubdivision, w => w.SelectedItem)
				.InitializeFromSource();

			entryAddress.IsEditable = ViewModel.CanEdit;
			entryAddress.Binding
				.AddBinding(ViewModel.Entity, e => e.Address, w => w.Text)
				.InitializeFromSource();
			
			entryMovementNotificationsSubdivisionRecipient.Sensitive = ViewModel.CanEdit;
			entryMovementNotificationsSubdivisionRecipient.ViewModel = ViewModel.SubdivisionViewModel;

			btnSave.Sensitive = ViewModel.CanEdit;
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
