using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.Domain.Documents;
using System.Linq;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using Vodovoz.ReportsParameters;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities;

namespace Vodovoz.Views.Warehouse
{
	public partial class InventoryDocumentView : TabViewBase<InventoryDocumentViewModel>
	{
		public InventoryDocumentView(InventoryDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;
			btnAccept.Clicked += OnAcceptClicked;
			btnPrint.Clicked += OnPrintClicked;
			
			radioBtnBulkAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsBulkAccountingActive, w => w.Active)
				.InitializeFromSource();
			radioBtnInstanceAccounting.Binding
				.AddBinding(ViewModel, vm => vm.IsInstanceAccountingActive, w => w.Active)
				.InitializeFromSource();
			
			ydatepickerDocDate.Sensitive = hboxStorages.Sensitive = ytextviewCommnet.Editable = ViewModel.CanEditDocument;
			vboxNomenclatureItems.Sensitive = vboxNomenclatureInstanceItems.Sensitive = ViewModel.CanEditDocument;

			ydatepickerDocDate.Binding
				.AddBinding(ViewModel.Entity, e => e.TimeStamp, w => w.Date)
				.InitializeFromSource();

			enumCmbInventoryDocumentType.ItemsEnum = typeof(InventoryDocumentType);
			enumCmbInventoryDocumentType.Binding
				.AddBinding(ViewModel.Entity, e => e.InventoryDocumentType, w => w.SelectedItem)
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
			carStorageEntry.ViewModel = ViewModel.InventoryCarViewModel;

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
			
			if(!ViewModel.Entity.CanEdit && ViewModel.Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				//ydatepickerDocDate.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				//yentryrefWarehouse.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				//ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				btnSave.Sensitive = false;
				vboxNomenclatureItems.Sensitive = vboxNomenclatureInstanceItems.Sensitive = false;
			}
			else
			{
				ViewModel.Entity.CanEdit = true;
			}
		}

		private void OnPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}

		private void OnAcceptClicked(object sender, EventArgs e)
		{
			ViewModel.AcceptCommand.Execute();
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		#region объемный учет

		private void ConfigureBulkAccounting()
		{
			ConfigureNomenclatureItemsTree();
			
			btnFillNomenclatureItemsByStorage.Clicked += OnFillNomenclatureItemsByStorageClicked;
			btnAddMissingNomenclatures.Clicked += OnAddMissingNomenclaturesClicked;
			btnAddFineToNomenclatureItem.Clicked += OnAddFineToNomenclatureItemClicked;
			btnDeleteFineFromNomenclatureItem.Clicked += OnDeleteFineFromNomenclatureItemClicked;
			
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
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
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
					var color = new Color(255, 255, 255);
					if(ViewModel._nomenclaturesWithDiscrepancies.Any(x => x.Id == node.Nomenclature.Id))
					{
						color = new Color(255, 125, 125);
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			treeViewNomenclatureItems.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItems;
			treeViewNomenclatureItems.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclatureItem, w => w.SelectedRow)
				.InitializeFromSource();
		}

		#endregion

		#region Экземплярный учет

		private void ConfigureInstanceAccounting()
		{
			ConfigureNomenclatureInstanceItemsTree();
			
			btnFillNomenclatureInstanceItemsByStorage.Clicked += OnFillNomenclatureInstanceItemsByStorageClicked;
			btnAddMissingNomenclatureInstances.Clicked += OnAddMissingNomenclatureInstancesClicked;
			//btnAddFineToNomenclatureInstanceItem.Clicked += OnAddFineToNomenclatureItemClicked;
			//btnDeleteFineFromNomenclatureInstanceItem.Clicked += OnDeleteFineFromNomenclatureItemClicked;
			
			btnFillNomenclatureInstanceItemsByStorage.Binding
				.AddBinding(ViewModel, vm => vm.FillNomenclatureInstancesByStorageTitle, w => w.Label)
				.InitializeFromSource();
			btnAddFineToNomenclatureInstanceItem.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedNomenclatureItemHasFine, w => w.Label)
				.AddBinding(vm => vm.HasSelectedInstanceItem, w => w.Sensitive)
				.InitializeFromSource();
			btnDeleteFineFromNomenclatureInstanceItem.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclatureItemHasFine, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void OnAddMissingNomenclatureInstancesClicked(object sender, EventArgs e)
		{
			ViewModel.AddMissingNomenclatureInstanceCommand.Execute();
		}

		private void OnFillNomenclatureInstanceItemsByStorageClicked(object sender, EventArgs e)
		{
			ViewModel.FillNomenclatureItemsByStorageCommand.Execute();
		}

		private void ConfigureNomenclatureInstanceItemsTree()
		{
			treeViewInstanceItems.ColumnsConfig = ColumnsConfigFactory.Create<InstanceInventoryDocumentItem>()
				.AddColumn("Отсутствует")
					.AddToggleRenderer(x => x.IsMissing)
					.Editing()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => ViewModel.GetNomenclatureName(x.InventoryNomenclatureInstance.Nomenclature), useMarkup: true)
				.AddColumn("Штраф")
					.AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Что произошло")
					.AddTextRenderer(x => x.Comment)
					.Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = new Color(255, 255, 255);
					if(ViewModel._nomenclaturesWithDiscrepancies.Any(x => x.Id == node.InventoryNomenclatureInstance.Nomenclature.Id))
					{
						color = new Color(255, 125, 125);
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			treeViewInstanceItems.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItems;
			treeViewInstanceItems.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclatureInstanceItem, w => w.SelectedRow)
				.InitializeFromSource();
			
			btnAddFineToNomenclatureInstanceItem.Binding
				.AddBinding(ViewModel, vm => vm.AddOrEditInstanceItemFineTitle, w => w.Label)
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
