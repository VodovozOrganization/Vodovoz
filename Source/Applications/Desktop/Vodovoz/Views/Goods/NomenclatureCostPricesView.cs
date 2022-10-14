using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureCostPricesView : WidgetViewBase<NomenclatureCostPricesViewModel>
	{
		public NomenclatureCostPricesView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			treeViewPurchasePrices.ColumnsConfig = FluentColumnsConfig<NomenclatureCostPriceViewModel>.Create()
				.AddColumn("Цена\nзакупки").AddNumericRenderer(x => x.CostPrice)
					.WidthChars(10)
					.Digits(2)
					.Editing(x => x.CanEditPrice)
					.Adjustment(new Gtk.Adjustment(0, 0, 999999999, 1, 10, 10))
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDateTitle)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDateTitle)
				.Finish();
			treeViewPurchasePrices.Binding.AddSource(ViewModel)
				.AddBinding( vm => vm.PriceViewModels, w => w.ItemsDataSource)
				.AddBinding( vm => vm.SelectedPrice, w => w.SelectedRow)
				.InitializeFromSource();

			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonChangePurchasePrice.Clicked += (sender, e) => ViewModel.CreatePriceCommand.Execute();
			ViewModel.CreatePriceCommand.CanExecuteChanged += (sender, e) => buttonChangePurchasePrice.Sensitive = ViewModel.CreatePriceCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangeDateCommand.Execute();
			ViewModel.ChangeDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangeDateCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();
		}
	}
}
