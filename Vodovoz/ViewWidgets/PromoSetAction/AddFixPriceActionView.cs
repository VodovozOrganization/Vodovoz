using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.ViewWidgets.PromoSetAction
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddFixPriceActionView : WidgetViewBase<AddFixPriceActionViewModel>
	{
		public AddFixPriceActionView(AddFixPriceActionViewModel viewModel)
		{
			this.Build();
			ViewModel = viewModel;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entityviewmodelentryNomenclature.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(ServicesConfig.CommonServices)
			);
			entityviewmodelentryNomenclature.Binding.AddBinding(ViewModel, vm => vm.Nomenclature, w => w.Subject);
			entityviewmodelentryNomenclature.CanEditReference = true;

			yspinbuttonPrice.Binding.AddBinding(ViewModel, vm => vm.Price, w => w.ValueAsDecimal);

			buttonCancel.Clicked += (sender, e) => { ViewModel.CancelCommand.Execute(); };
			ybuttonAccept.Clicked += (sender, e) => { ViewModel.AcceptCommand.Execute(); };
		}

	}
}
