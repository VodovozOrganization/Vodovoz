using System;
using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Documents;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class ShiftChangeResidueDocumentView : TabViewBase<ShiftChangeResidueDocumentViewModel>
	{
		public ShiftChangeResidueDocumentView(ShiftChangeResidueDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;
			buttonPrint.Clicked += OnPrintClicked;
			
			ydatepickerDocDate.Sensitive = ytextviewCommnet.Editable = ViewModel.CanEdit || ViewModel.CanCreate;

			enumCmbShiftChangeResidueTypeByStorage.ItemsEnum = typeof(ShiftChangeResidueDocumentType);
			enumCmbShiftChangeResidueTypeByStorage.Binding
				.AddBinding(ViewModel.Entity, e => e.ShiftChangeResidueDocumentType, w => w.SelectedItem)
				.InitializeFromSource();

			ydatepickerDocDate.Binding
				.AddBinding(ViewModel.Entity, e => e.TimeStamp, w => w.Date)
				.InitializeFromSource();
			ytextviewCommnet.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			radioBtnBulkAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsBulkAccountingActive, w => w.Active)
				.InitializeFromSource();
			radioBtnInstanceAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsInstanceAccountingActive, w => w.Active)
				.InitializeFromSource();
			
			notebookItems.ShowTabs = false;
			notebookItems.Binding
				.AddBinding(ViewModel, vm => vm.ActiveAccounting, w => w.CurrentPage)
				.InitializeFromSource();

			employeeSenderEntry.ViewModel = ViewModel.EmployeeSenderEntryViewModel;
			employeeReceiverEntry.ViewModel = ViewModel.EmployeeReceiverEntryViewModel;
			lblWarehouseStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseStorage, w => w.Visible)
				.InitializeFromSource();
			warehouseStorageEntry.ViewModel = ViewModel.WarehouseStorageEntryViewModel;
			warehouseStorageEntry.ViewModel.IsEditable = ViewModel.CanEdit || ViewModel.CanCreate;
			warehouseStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseStorage, w => w.Visible)
				.InitializeFromSource();
			lblCarStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarStorage, w => w.Visible)
				.InitializeFromSource();
			carStorageEntry.ViewModel = ViewModel.CarStorageEntryViewModel;
			carStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarStorage, w => w.Visible)
				.InitializeFromSource();
			
			ConfigureSelectableFilter();
			ConfigureWidgetsForItems();
		}
		
		private void ConfigureSelectableFilter()
		{
			var filterWidget = new SelectableParameterReportFilterView(ViewModel.SelectableFilterViewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		private void ConfigureWidgetsForItems()
		{
			#region Объемный учет

			ConfigureNomenclatureItemsTree();
			
			btnFillNomenclatureItemsByStorage.Clicked += OnFillNomenclatureItemsByStorageClicked;
			btnFillNomenclatureItemsByStorage.Binding
				.AddBinding(ViewModel, vm => vm.FillNomenclaturesByStorageTitle, w => w.Label)
				.InitializeFromSource();

			#endregion
			
			#region Экземплярный учет

			ConfigureInstanceItemsTree();
			
			btnFillInstanceItemsByStorage.Clicked += OnFillInstanceItemsByStorageClicked;
			btnFillInstanceItemsByStorage.Binding
				.AddBinding(ViewModel, vm => vm.FillNomenclatureInstancesByStorageTitle, w => w.Label)
				.InitializeFromSource();

			#endregion
		}

		private void ConfigureNomenclatureItemsTree()
		{
			treeNomenclatureItems.ColumnsConfig = FluentColumnsConfig<ShiftChangeWarehouseDocumentItem>.Create()
				.AddColumn("Номенклатура")
				.AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во в учёте")
				.AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту")
				.AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница")
				.AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба")
				.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Что произошло")
				.AddTextRenderer(x => x.Comment).Editable()
				.Finish();
			
			treeNomenclatureItems.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItems;
			//treeNomenclatureItems.YTreeModel?.EmitModelChanged();
		}

		private void ConfigureInstanceItemsTree()
		{
			treeInstanceItems.ColumnsConfig = FluentColumnsConfig<InstanceShiftChangeWarehouseDocumentItem>.Create()
				.AddColumn("Отсутствует")
				.AddToggleRenderer(x => x.IsMissing)
				.AddSetter((c, n) => c.Activatable = n.CanChangeIsMissing)
				.AddColumn("Номенклатура")
				.AddTextRenderer(x => x.InventoryNomenclatureInstance.Name)
				.AddColumn("Инвентарный номер")
				.AddTextRenderer(x => x.InventoryNomenclatureInstance.InventoryNumber)
				/*.AddColumn("Кол-во в учёте")
					.AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту")
					.AddNumericRenderer(x => x.AmountInFact).Editing()
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
					.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница")
					.AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
					.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))*/
				.AddColumn("Что произошло")
				.AddTextRenderer(x => x.Comment).Editable()
				.AddColumn("Описание расхождения")
				.AddTextRenderer(x => x.DiscrepancyDescription)
				.Finish();
			
			treeInstanceItems.ItemsDataSource = ViewModel.Entity.ObservableInstanceItems;
		}

		#region Button handlers

		private void OnFillNomenclatureItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureItemsByStorageCommand.Execute();
		}
		
		private void OnFillInstanceItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureInstanceItemsCommand.Execute();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}
		
		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}
		
		private void OnPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}

		#endregion
	}
}
