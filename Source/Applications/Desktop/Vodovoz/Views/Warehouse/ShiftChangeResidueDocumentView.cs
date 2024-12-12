using System;
using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Documents;
using Vodovoz.Infrastructure;
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
			
			ydatepickerDocDate.Sensitive = ViewModel.CanEdit || ViewModel.CanCreate;
			ytextviewCommnet.Editable = ViewModel.CanEdit || ViewModel.CanCreate;

			enumCmbShiftChangeResidueTypeByStorage.ItemsEnum = typeof(ShiftChangeResidueDocumentType);
			enumCmbShiftChangeResidueTypeByStorage.Binding
				.AddBinding(ViewModel.Entity, e => e.ShiftChangeResidueDocumentType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeShiftChangeResidueDocumentType, w => w.Sensitive)
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

			ychkSortNomenclaturesByTitle.Binding
				.AddBinding(ViewModel.Entity, e => e.SortedByNomenclatureName, w => w.Active)
				.InitializeFromSource();

			employeeSenderEntry.ViewModel = ViewModel.EmployeeSenderEntryViewModel;
			employeeSenderEntry.ViewModel.IsEditable = ViewModel.CanEdit || ViewModel.CanCreate;
			employeeReceiverEntry.ViewModel = ViewModel.EmployeeReceiverEntryViewModel;
			employeeReceiverEntry.ViewModel.IsEditable = ViewModel.CanEdit || ViewModel.CanCreate;
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
			carStorageEntry.ViewModel.IsEditable = ViewModel.CanEdit || ViewModel.CanCreate;
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

			vboxNomenclatureItems.Sensitive = ViewModel.CanEdit || ViewModel.CanCreate;
			ConfigureNomenclatureItemsTree();
			
			btnFillNomenclatureItemsByStorage.Clicked += OnFillNomenclatureItemsByStorageClicked;
			btnAddMissingNomenclatures.Clicked += OnAddMissingNomenclaturesClicked;
			
			btnFillNomenclatureItemsByStorage.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FillNomenclaturesByStorageTitle, w => w.Label)
				.AddBinding(vm => vm.CanHandleDocumentItems, w => w.Sensitive)
				.InitializeFromSource();
				
			btnAddMissingNomenclatures.Binding
				.AddBinding(ViewModel, vm => vm.CanHandleDocumentItems, w => w.Sensitive)
				.InitializeFromSource();

			#endregion
			
			#region Экземплярный учет

			vboxInstanceItems.Sensitive = ViewModel.CanEdit || ViewModel.CanCreate;
			ConfigureInstanceItemsTree();
			
			btnFillInstanceItemsByStorage.Clicked += OnFillInstanceItemsByStorageClicked;
			btnAddMissingInstances.Clicked += OnAddMissingInstancesClicked;
			
			btnFillInstanceItemsByStorage.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FillNomenclatureInstancesByStorageTitle, w => w.Label)
				.AddBinding(vm => vm.CanHandleDocumentItems, w => w.Sensitive)
				.InitializeFromSource();
			
			btnAddMissingInstances.Binding
				.AddBinding(ViewModel, vm => vm.CanHandleDocumentItems, w => w.Sensitive)
				.InitializeFromSource();

			expanderInstancesDiscrepancies.Expanded = true;
			txtViewInstancesDiscrepancies.Editable = false;
			txtViewInstancesDiscrepancies.Binding
				.AddBinding(ViewModel, vm => vm.InstancesDiscrepanciesString, w => w.Buffer.Text)
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
					.AddSetter((w, x) => w.ForegroundGdk = x.Difference < 0 ? GdkColors.DangerText : GdkColors.InfoText)
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Что произошло")
					.AddTextRenderer(x => x.Comment).Editable()
				.Finish();
			
			treeNomenclatureItems.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItems;
		}

		private void ConfigureInstanceItemsTree()
		{
			treeInstanceItems.ColumnsConfig = FluentColumnsConfig<InstanceShiftChangeWarehouseDocumentItem>.Create()
				.AddColumn("Отсутствует")
					.AddToggleRenderer(x => x.IsMissing)
					.AddSetter((c, n) => c.Activatable = n.CanChangeIsMissing)
				.AddColumn("Кол-во в учёте")
					.AddTextRenderer(x => x.InventoryNomenclatureInstance.Nomenclature != null
						? x.InventoryNomenclatureInstance.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB)
						: x.AmountInDB.ToString())
				.AddColumn("Разница")
					.AddTextRenderer(x =>
						x.Difference != 0 && x.InventoryNomenclatureInstance.Nomenclature.Unit != null
							? x.InventoryNomenclatureInstance.Nomenclature.Unit.MakeAmountShortStr(x.Difference)
							: string.Empty)
					.AddSetter((w, x) => w.ForegroundGdk = x.Difference < 0 ? GdkColors.DangerText : GdkColors.InfoText)
				.AddColumn("Экземпляр")
					.AddTextRenderer(x => x.InventoryNomenclatureInstance.Name)
				.AddColumn("Инвентарный номер")
					.AddTextRenderer(x => x.InventoryNomenclatureInstance.GetInventoryNumber)
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Что произошло")
					.AddTextRenderer(x => x.Comment).Editable()
				.Finish();
			
			treeInstanceItems.ItemsDataSource = ViewModel.Entity.ObservableInstanceItems;
		}

		#region Button handlers

		private void OnFillNomenclatureItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureItemsByStorageCommand.Execute();
		}
		
		private void OnAddMissingNomenclaturesClicked(object sender, EventArgs e)
		{
			ViewModel.AddMissingNomenclatureCommand.Execute();
		}
		
		private void OnFillInstanceItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureInstanceItemsCommand.Execute();
		}
		
		private void OnAddMissingInstancesClicked(object sender, EventArgs e)
		{
			ViewModel.AddMissingNomenclatureInstanceCommand.Execute();
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
