using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclaturePurchasePricesView : WidgetViewBase<NomenclaturePurchasePricesViewModel>
	{
		public NomenclaturePurchasePricesView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			treeViewPurchasePrices.ColumnsConfig = FluentColumnsConfig<NomenclatureCostPurchasePriceViewModel>.Create()
				.AddColumn("Цена").AddNumericRenderer(x => x.PurchasePrice)
					.Digits(2)
					.Editing(x => x.CanEditPrice)
					.Adjustment(new Gtk.Adjustment(0, 0, 999999999, 1, 10, 10))
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDateTitle)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDateTitle)
				.Finish();
			treeViewPurchasePrices.Binding.AddBinding(ViewModel, vm => vm.PriceViewModels, w => w.ItemsDataSource).InitializeFromSource();
			treeViewPurchasePrices.Selection.Changed += Selection_Changed;

			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonChangePurchasePrice.Clicked += (sender, e) => ViewModel.CreatePriceCommand.Execute();
			ViewModel.CreatePriceCommand.CanExecuteChanged += (sender, e) => buttonChangePurchasePrice.Sensitive = ViewModel.CreatePriceCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangeDateCommand.Execute();
			ViewModel.ChangeDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangeDateCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();
		}

		private void Selection_Changed(object sender, System.EventArgs e)
		{
			var selectedPrice = treeViewPurchasePrices.GetSelectedObject<NomenclatureCostPurchasePriceViewModel>();
			if(selectedPrice == null)
			{
				return;
			}

			ViewModel.SelectedPrice = selectedPrice;
		}
	}
}
