using QS.Views.GtkUI;
using Vodovoz.ViewModels.Store;

namespace Vodovoz.Store
{
	public partial class RegradingOfGoodsTemplateView : TabViewBase<RegradingOfGoodsTemplateViewModel>
	{
		public RegradingOfGoodsTemplateView(RegradingOfGoodsTemplateViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			regradingofgoodstemplateitemsview1.ViewModel = ViewModel.ItemsViewModel;

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
