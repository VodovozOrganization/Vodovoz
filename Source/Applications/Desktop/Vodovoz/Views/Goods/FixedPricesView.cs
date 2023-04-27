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
			ytreeviewNomenclatures.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewNomenclatures.Binding.AddBinding(ViewModel, vm => vm.FixedPriceNomenclatures, w => w.ItemsDataSource).InitializeFromSource();
			ytreeviewNomenclatures.Binding.AddBinding(ViewModel, vm => vm.SelectedNomenclature, w => w.SelectedRow).InitializeFromSource();

			ViewModel.DiffFormatter = new PangoDiffFormater();
			ytreeviewFixedPricesChanges.ColumnsConfig = FluentColumnsConfig<FieldChange>.Create()
				.AddColumn("Время\nизменения").AddTextRenderer(x => x.Entity.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.Entity.ChangeSet.UserName)
				.AddColumn("Старое\nзначение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
				.AddColumn("Новое\nзначение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
				.AddColumn("Параметр").AddTextRenderer(x => x.FieldTitle)
				.Finish();

			ytreeviewFixedPricesChanges.Binding.AddBinding(ViewModel, vm => vm.SelectedPriceChanges, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewFixedPriceAndCount.ColumnsConfig = FluentColumnsConfig<FixedPriceItemViewModel>.Create()
				.AddColumn("Минимальное\nколичество").AddNumericRenderer(x => x.MinCount).Editing()
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.AddColumn("Фиксированная\nцена").AddNumericRenderer(x => x.FixedPrice).Editing().Digits(2)
					.AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.Finish();

			ytreeviewFixedPriceAndCount.Binding.AddBinding(ViewModel, vm => vm.FixedPricesByNomenclature, w => w.ItemsDataSource).InitializeFromSource();
			ytreeviewFixedPriceAndCount.Binding.AddBinding(ViewModel, vm => vm.SelectedFixedPrice, w => w.SelectedRow).InitializeFromSource();

			buttonAdd.Clicked += (s, e) => ViewModel.AddNomenclatureCommand.Execute();
			ViewModel.AddNomenclatureCommand.CanExecuteChanged += (sender, e) => buttonAdd.Sensitive = ViewModel.AddNomenclatureCommand.CanExecute();
			buttonAdd.Sensitive = ViewModel.AddNomenclatureCommand.CanExecute();

			buttonAddFixedPrice.Clicked += (s, e) => ViewModel.AddFixedPriceCommand.Execute();
			ViewModel.AddFixedPriceCommand.CanExecuteChanged += (sender, e) => buttonAddFixedPrice.Sensitive = ViewModel.AddFixedPriceCommand.CanExecute();
			buttonAddFixedPrice.Sensitive = ViewModel.AddFixedPriceCommand.CanExecute();

			buttonDel.Clicked += (s, e) => ViewModel.RemoveFixedPriceCommand.Execute();
			ViewModel.RemoveFixedPriceCommand.CanExecuteChanged += (sender, e) => buttonDel.Sensitive = ViewModel.RemoveFixedPriceCommand.CanExecute();
			buttonDel.Sensitive = ViewModel.RemoveFixedPriceCommand.CanExecute();
		}
	}
}
