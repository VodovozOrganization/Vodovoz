using System;
using QS.Navigation;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class InventoryInstanceView : ViewBase<InventoryInstanceViewModel>
	{
		public InventoryInstanceView(InventoryInstanceViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnSave.Sensitive = ViewModel.CanEdit;
			btnCancel.Clicked += OnCancelClicked;

			entryNomenclature.ViewModel = ViewModel.NomenclatureViewModel;
			entryNomenclature.Sensitive = ViewModel.CanEditNewEntity;
			
			inventoryNumberEntry.Binding
				.AddBinding(ViewModel.Entity, e => e.InventoryNumber, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditNewEntity, w => w.Sensitive)
				.InitializeFromSource();

			spinBtnPurchasePrice.Sensitive = false;
			spinBtnPurchasePrice.Binding
				.AddBinding(ViewModel.Entity, vm => vm.PurchasePrice, w => w.ValueAsDecimal)
				.InitializeFromSource();
			
			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			lblUsed.Binding
				.AddBinding(ViewModel, vm => vm.CanShowUsedPrefix, w => w.Visible)
				.InitializeFromSource();
			lblIsUsed.Binding
				.AddBinding(ViewModel, vm => vm.CanShowIsUsed, w => w.Visible)
				.InitializeFromSource();
			chkIsUsed.Binding
				.AddBinding(ViewModel.Entity, e => e.IsUsed, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditUsedParameter, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanShowIsUsed, w => w.Visible)
				.InitializeFromSource();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}
		
		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
