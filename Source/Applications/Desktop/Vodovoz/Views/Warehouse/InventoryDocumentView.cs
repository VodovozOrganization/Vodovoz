using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using System.Linq;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using Vodovoz.ReportsParameters;
using QS.Navigation;
using QS.Utilities;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Infrastructure;

namespace Vodovoz.Views.Warehouse
{
	public partial class InventoryDocumentView : TabViewBase<InventoryDocumentViewModel>
	{
		public InventoryDocumentView(InventoryDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			vbox4.Sensitive = ViewModel.CanEdit;
			
			ConfigureCommonButtons();
			
			lblDocumentStatus.Binding
				.AddBinding(ViewModel, e => e.DocumentStatus, w => w.LabelProp)
				.InitializeFromSource();
			
			radioBtnBulkAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsBulkAccountingActive, w => w.Active)
				.InitializeFromSource();
			radioBtnInstanceAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsInstanceAccountingActive, w => w.Active)
				.InitializeFromSource();

			ydatepickerDocDate.Binding
				.AddBinding(ViewModel.Entity, e => e.TimeStamp, w => w.Date)
				.InitializeFromSource();

			enumCmbInventoryDocumentType.ItemsEnum = typeof(InventoryDocumentType);
			enumCmbInventoryDocumentType.Binding
				.AddBinding(ViewModel.Entity, e => e.InventoryDocumentType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeInventoryDocumentType, w => w.Sensitive)
				.InitializeFromSource();

			hboxWarehouseStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseStorage, w => w.Visible)
				.InitializeFromSource();
			hboxEmployeeStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowEmployeeStorage, w => w.Visible)
				.InitializeFromSource();
			hboxCarStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarStorage, w => w.Visible)
				.InitializeFromSource();

			warehouseStorageEntry.ViewModel = ViewModel.InventoryWarehouseViewModel;
			employeeStorageEntry.ViewModel = ViewModel.InventoryEmployeeViewModel;
			employeeStorageEntry.Sensitive = ViewModel.HasAccessToEmployeeStorages;
			carStorageEntry.ViewModel = ViewModel.InventoryCarViewModel;
			carStorageEntry.Sensitive = ViewModel.HasAccessToCarStorages;

			ychkSortNomenclaturesByTitle.Binding
				.AddBinding(ViewModel.Entity, e => e.SortedByNomenclatureName, w => w.Active)
				.InitializeFromSource();

			ConfigureSelectableFilter();
			
			ytextviewCommnet.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			notebookItems.ShowTabs = false;
			notebookItems.Binding
				.AddBinding(ViewModel, vm => vm.ActiveAccounting, w => w.CurrentPage)
				.InitializeFromSource();

			ConfigureBulkAccounting();
			ConfigureInstanceAccounting();
		}

		private void ConfigureCommonButtons()
		{
			btnSave.Sensitive = ViewModel.CanEdit;
			btnConfirm.Sensitive = ViewModel.CanEdit;
			
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;
			btnConfirm.Clicked += OnConfirmClicked;
			btnPrint.Clicked += OnPrintClicked;
		}

		private void OnPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}

		private void OnConfirmClicked(object sender, EventArgs e)
		{
			ViewModel.ConfirmCommand.Execute();
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		#region Объемный учет

		private void ConfigureBulkAccounting()
		{
			ConfigureNomenclatureItemsTree();
			
			btnFillNomenclatureItemsByStorage.Clicked += OnFillNomenclatureItemsByStorageClicked;
			btnAddMissingNomenclatures.Clicked += OnAddMissingNomenclaturesClicked;
			btnAddFineToNomenclatureItem.Clicked += OnAddFineToNomenclatureItemClicked;
			btnDeleteFineFromNomenclatureItem.Clicked += OnDeleteFineFromNomenclatureItemClicked;
			btnFillByAccounting.Clicked += OnFillByAccountingClicked;
			
			hboxHandleNomenclatureItemsBtns.Binding
				.AddBinding(ViewModel, vm => vm.CanHandleInventoryItems, w => w.Sensitive)
				.InitializeFromSource();
			
			btnFillNomenclatureItemsByStorage.Binding
				.AddBinding(ViewModel, vm => vm.FillNomenclaturesByStorageTitle, w => w.Label)
				.InitializeFromSource();
			
			btnAddFineToNomenclatureItem.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AddOrEditNomenclatureItemFineTitle, w => w.Label)
				.AddBinding(vm => vm.HasSelectedNomenclatureItem, w => w.Sensitive)
				.InitializeFromSource();
			btnDeleteFineFromNomenclatureItem.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclatureItemHasFine, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void OnFillNomenclatureItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureItemsByStorageCommand.Execute();
		}
		
		private void OnAddMissingNomenclaturesClicked(object sender, EventArgs e)
		{
			ViewModel.AddMissingNomenclatureCommand.Execute();
		}
		
		private void OnAddFineToNomenclatureItemClicked(object sender, EventArgs e)
		{
			ViewModel.AddOrEditNomenclatureItemFineCommand.Execute();
		}
		
		private void OnDeleteFineFromNomenclatureItemClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineFromNomenclatureItemCommand.Execute();
		}
		
		private void OnFillByAccountingClicked(object sender, EventArgs e)
		{
			ViewModel.FillFactByAccountingCommand.Execute();
		}

		private void ConfigureNomenclatureItemsTree()
		{
			treeViewNomenclatureItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => ViewModel.GetNomenclatureName(x.Nomenclature), useMarkup: true)
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
				.AddColumn("Штраф")
					.AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Что произошло")
					.AddTextRenderer(x => x.Comment)
					.Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = GdkColors.PrimaryBase;
					if(ViewModel.NomenclaturesWithDiscrepancies.Any(x => x.Id == node.Nomenclature.Id))
					{
						color = GdkColors.Pink;
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			treeViewNomenclatureItems.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItems;
			treeViewNomenclatureItems.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclatureItem, w => w.SelectedRow)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}

		#endregion

		#region Экземплярный учет

		private void ConfigureInstanceAccounting()
		{
			ConfigureNomenclatureInstanceItemsTree();
			
			btnFillNomenclatureInstanceItemsByStorage.Clicked += OnFillNomenclatureInstanceItemsByStorageClicked;
			btnAddMissingNomenclatureInstances.Clicked += OnAddMissingNomenclatureInstancesClicked;
			btnAddFineToNomenclatureInstanceItem.Clicked += OnAddFineToNomenclatureInstanceItemClicked;
			btnDeleteFineFromNomenclatureInstanceItem.Clicked += OnDeleteFineFromNomenclatureInstanceItemClicked;
			
			hboxHandleInstanceItemsBtns.Binding
				.AddBinding(ViewModel, vm => vm.CanHandleInventoryItems, w => w.Sensitive)
				.InitializeFromSource();
			
			btnFillNomenclatureInstanceItemsByStorage.Binding
				.AddBinding(ViewModel, vm => vm.FillNomenclatureInstancesByStorageTitle, w => w.Label)
				.InitializeFromSource();
			btnAddFineToNomenclatureInstanceItem.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AddOrEditInstanceItemFineTitle, w => w.Label)
				.AddBinding(vm => vm.HasSelectedInstanceItem, w => w.Sensitive)
				.InitializeFromSource();
			btnDeleteFineFromNomenclatureInstanceItem.Binding
				.AddBinding(ViewModel, vm => vm.SelectedInstanceItemHasFine, w => w.Sensitive)
				.InitializeFromSource();

			expanderInstancesDiscrepancies.Expanded = true;
			txtViewInstancesDiscrepancies.Editable = false;
			txtViewInstancesDiscrepancies.Binding
				.AddBinding(ViewModel, vm => vm.InstancesDiscrepanciesString, w => w.Buffer.Text)
				.InitializeFromSource();
		}

		private void OnAddMissingNomenclatureInstancesClicked(object sender, EventArgs e)
		{
			ViewModel.AddMissingNomenclatureInstanceCommand.Execute();
		}

		private void OnFillNomenclatureInstanceItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureInstanceItemsCommand.Execute();
		}
		
		private void OnAddFineToNomenclatureInstanceItemClicked(object sender, EventArgs e)
		{
			ViewModel.AddOrEditNomenclatureInstanceItemFineCommand.Execute();
		}
		
		private void OnDeleteFineFromNomenclatureInstanceItemClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineToNomenclatureInstanceItemCommand.Execute();
		}

		private void ConfigureNomenclatureInstanceItemsTree()
		{
			treeViewInstanceItems.ColumnsConfig = ColumnsConfigFactory.Create<InstanceInventoryDocumentItem>()
				.AddColumn("Отсутствует")
					.AddToggleRenderer(x => x.IsMissing)
					.AddSetter((n, c) => n.Activatable = c.CanChangeIsMissing)
				.AddColumn("Кол-во в учёте")
					.AddTextRenderer(
						x => x.InventoryNomenclatureInstance.Nomenclature.Unit != null
							? x.InventoryNomenclatureInstance.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Экземпляр")
					.AddTextRenderer(x => x.InventoryNomenclatureInstance.ToString(), useMarkup: true)
				.AddColumn("Разница")
					.AddTextRenderer(x =>
						x.Difference != 0 && x.InventoryNomenclatureInstance.Nomenclature.Unit != null
							? x.InventoryNomenclatureInstance.Nomenclature.Unit.MakeAmountShortStr(x.Difference)
							: string.Empty)
					.AddSetter((w, x) => w.ForegroundGdk = x.Difference < 0 ? GdkColors.DangerText : GdkColors.InfoText)
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф")
					.AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Что произошло")
					.AddTextRenderer(x => x.Comment)
					.Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = GdkColors.PrimaryBase;
					if(node.IsMissing && node.AmountInDB > 0)
					{
						color = GdkColors.Pink;
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			treeViewInstanceItems.ItemsDataSource = ViewModel.Entity.ObservableInstanceItems;
			treeViewInstanceItems.Binding
				.AddBinding(ViewModel, vm => vm.SelectedInstanceItem, w => w.SelectedRow)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}

		#endregion

		private void ConfigureSelectableFilter()
		{
			var filterWidget = new SelectableParameterReportFilterView(ViewModel.SelectableFilterViewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}
	}
}
