using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
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
			btnSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			ConfigureNomenclatureEntry();
			
			spinBtnPurchasePrice.Binding
				.AddBinding(ViewModel.Entity, vm => vm.PurchasePrice, w => w.ValueAsDecimal)
				.InitializeFromSource();
			spinBtnCostPrice.Binding
				.AddBinding(ViewModel.Entity, vm => vm.CostPrice, w => w.ValueAsDecimal)
				.InitializeFromSource();
			spinBtnInnerDeliveryPrice.Binding
				.AddBinding(ViewModel.Entity, vm => vm.InnerDeliveryPrice, w => w.ValueAsDecimal)
				.InitializeFromSource();
		}

		private void ConfigureNomenclatureEntry()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryNomenclatureInstance>(
				ViewModel, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);

			entryNomenclature.ViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelDialog<NomenclatureViewModel>()
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.Finish();
		}
	}
}
