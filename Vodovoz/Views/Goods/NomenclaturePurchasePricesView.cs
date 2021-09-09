using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.WageCalculation;

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
			treeViewPurchasePrices.ColumnsConfig = FluentColumnsConfig<NomenclaturePurchasePrice>.Create()
				.AddColumn("Цена").AddNumericRenderer(x => $"{x.PurchasePrice:N2} ₽")
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate.ToString("G"))
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("G") : "")
				.Finish();

			treeViewPurchasePrices.ItemsDataSource = ViewModel.Entity.ObservablePurchasePrices;

			treeViewPurchasePrices.RowActivated += (o, args) => 
				ViewModel.OpenPurchasePriceCommand.Execute(GetSelectedNode());
			
			treeViewPurchasePrices.Selection.Changed += (sender, e) =>
			{
				ViewModel.ChangePurchasePriceCommand.RaiseCanExecuteChanged();
				ViewModel.ChangePurchasePriceStartDateCommand.RaiseCanExecuteChanged();
				ViewModel.OpenPurchasePriceCommand.RaiseCanExecuteChanged();
			};

			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonChangePurchasePrice.Clicked += (sender, e) => ViewModel.ChangePurchasePriceCommand.Execute();
			ViewModel.ChangePurchasePriceCommand.CanExecuteChanged += (sender, e) => buttonChangePurchasePrice.Sensitive = ViewModel.ChangePurchasePriceCommand.CanExecute();
			buttonChangePurchasePrice.Sensitive = ViewModel.ChangePurchasePriceCommand.CanExecute();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangePurchasePriceStartDateCommand.Execute(GetSelectedNode());
			ViewModel.ChangePurchasePriceStartDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangePurchasePriceStartDateCommand.CanExecute(GetSelectedNode());
			buttonChangeDate.Sensitive = ViewModel.ChangePurchasePriceStartDateCommand.CanExecute(GetSelectedNode());
		}

		private NomenclaturePurchasePrice GetSelectedNode()
		{
			return treeViewPurchasePrices.GetSelectedObject() as NomenclaturePurchasePrice;
		}
	}
}
