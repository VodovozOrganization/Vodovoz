using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;
using Gamma.ColumnConfig;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using QS.HistoryLog.Domain;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FixedPricesView : WidgetViewBase<FixedPricesViewModel>
	{
		public FixedPricesView() : base()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
			if(ViewModel == null) {
				return;
			}

			Configure();
		}

		private void Configure()
		{
			ytreeviewFixedPrices.ColumnsConfig = FluentColumnsConfig<FixedPriceItemViewModel>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.NomenclatureTitle)
				.AddColumn("Фиксированная цена").AddNumericRenderer(x => x.FixedPrice).Editing().Digits(2)
					.AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.Finish();

			ViewModel.DiffFormatter = new PangoDiffFormater();
			ytreeviewFixedPricesChanges.ColumnsConfig = FluentColumnsConfig<FieldChange>.Create()
				.AddColumn("Время изменения").AddTextRenderer(x => x.Entity.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.Entity.ChangeSet.UserName)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
				.Finish();

			ytreeviewFixedPrices.Selection.Changed += PriceSelection_Changed;
			ytreeviewFixedPrices.Binding.AddBinding(ViewModel, vm => vm.FixedPrices, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewFixedPricesChanges.Binding.AddBinding(ViewModel, vm => vm.SelectedPriceChanges, w => w.ItemsDataSource).InitializeFromSource();

			buttonAdd.Clicked += (s, e) => ViewModel.AddFixedPriceCommand.Execute();
			ViewModel.AddFixedPriceCommand.CanExecuteChanged += (sender, e) => buttonAdd.Sensitive = ViewModel.AddFixedPriceCommand.CanExecute();
			buttonAdd.Sensitive = ViewModel.AddFixedPriceCommand.CanExecute();

			buttonDel.Clicked += (s, e) => ViewModel.RemoveFixedPriceCommand.Execute();
			ViewModel.RemoveFixedPriceCommand.CanExecuteChanged += (sender, e) => buttonDel.Sensitive = ViewModel.RemoveFixedPriceCommand.CanExecute();
			buttonDel.Sensitive = ViewModel.RemoveFixedPriceCommand.CanExecute();
		}

		void PriceSelection_Changed(object sender, EventArgs e)
		{
			var selectedPrice = ytreeviewFixedPrices.GetSelectedObject() as FixedPriceItemViewModel;
			ViewModel.SelectedFixedPrice = selectedPrice;
		}
	}
}
