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
			btnCancel.Clicked += OnCancelClicked;

			entryNomenclature.ViewModel = ViewModel.NomenclatureViewModel;
			
			inventoryNumberEntry.Binding
				.AddBinding(ViewModel.Entity, e => e.InventoryNumber, w => w.Text)
				.InitializeFromSource();
			
			spinBtnPurchasePrice.Binding
				.AddBinding(ViewModel.Entity, vm => vm.PurchasePrice, w => w.ValueAsDecimal)
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
