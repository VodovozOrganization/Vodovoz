using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureInnerDeliveryPricesView : WidgetViewBase<NomenclatureInnerDeliveryPricesViewModel>
	{
		public NomenclatureInnerDeliveryPricesView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			treeViewPrices.ColumnsConfig = FluentColumnsConfig<NomenclatureInnerDeliveryPriceViewModel>.Create()
				.AddColumn("Цена").AddNumericRenderer(x => x.Price)
					.Digits(2)
					.Editing(x => x.CanEditPrice)
					.Adjustment(new Gtk.Adjustment(0, 0, 999999999, 1, 10, 10))
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDateTitle)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDateTitle)
				.Finish();
			treeViewPrices.Binding.AddBinding(ViewModel, vm => vm.PriceViewModels, w => w.ItemsDataSource).InitializeFromSource();
			treeViewPrices.Selection.Changed += Selection_Changed;

			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonChangePrice.Clicked += (sender, e) => ViewModel.CreatePriceCommand.Execute();
			ViewModel.CreatePriceCommand.CanExecuteChanged += (sender, e) => buttonChangePrice.Sensitive = ViewModel.CreatePriceCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangeDateCommand.Execute();
			ViewModel.ChangeDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangeDateCommand.CanExecute();
			ViewModel.CreatePriceCommand.RaiseCanExecuteChanged();
		}

		private void Selection_Changed(object sender, System.EventArgs e)
		{
			var selectedPrice = treeViewPrices.GetSelectedObject<NomenclatureInnerDeliveryPriceViewModel>();
			if(selectedPrice == null)
			{
				return;
			}

			ViewModel.SelectedPrice = selectedPrice;
		}
	}
}
