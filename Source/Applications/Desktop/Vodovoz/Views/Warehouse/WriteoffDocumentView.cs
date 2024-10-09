using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class WriteoffDocumentView : TabViewBase<WriteOffDocumentViewModel>
	{
		public WriteoffDocumentView(WriteOffDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			buttonSave.Clicked += OnSaveClicked;
			buttonCancel.Clicked += OnCancelClicked;

			labelTimeStamp.Binding
				.AddBinding(ViewModel.Entity, e => e.DateString, w => w.LabelProp)
				.InitializeFromSource();
			
			responsibleEmployeeEntry.ViewModel = ViewModel.ResponsibleEmployeeViewModel;
			responsibleEmployeeEntry.ViewModel.IsEditable = ViewModel.CanEdit;
			textComment.Editable = ViewModel.CanEdit;
			buttonSave.Sensitive = ViewModel.CanEdit;
			
			comboType.ItemsEnum = typeof(WriteOffType);
			comboType.AddEnumToHideList(WriteOffType.Counterparty);
			comboType.Binding
				.AddBinding(ViewModel.Entity, e => e.WriteOffType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
				.InitializeFromSource();

			entryWarehouse.ViewModel = ViewModel.WarehouseViewModel;
			
			hboxStorages.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
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

			employeeStorageEntry.ViewModel = ViewModel.WriteOffFromEmployeeViewModel;
			employeeStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.HasAccessToEmployeeStorages, w => w.Sensitive)
				.InitializeFromSource();
			
			carStorageEntry.ViewModel = ViewModel.WriteOffFromCarViewModel;
			carStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.HasAccessToCarStorages, w => w.Sensitive)
				.InitializeFromSource();

			textComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			if(ViewModel.UserHasOnlyAccessToWarehouseAndComplaints)
			{
				responsibleEmployeeEntry.ViewModel.IsEditable = false;
			}
			
			vboxItems.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeItems, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeItems();

			btnAddNomenclature.Clicked += OnAddNomenclatureClicked;
			btnAddNomenclatureInstance.Clicked += OnAddNomenclatureInstanceClicked;
			btnDelete.Clicked += OnDeleteItemClicked;
			btnAddFine.Clicked += OnAddFineClicked;
			btnDeleteFine.Clicked += OnDeleteFineClicked;
			
			btnDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanDeleteItem, w => w.Sensitive)
				.InitializeFromSource();
			btnAddFine.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AddOrEditFineTitle, w => w.Label)
				.AddFuncBinding(vm => vm.SelectedItem != null, w => w.Sensitive)
				.InitializeFromSource();
			btnDeleteFine.Binding
				.AddBinding(ViewModel, vm => vm.HasSelectedFine, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void ConfigureTreeItems()
		{
			treeItemsList.ColumnsConfig = ColumnsConfigFactory.Create<WriteOffDocumentItem>()
				.AddColumn("Наименование")
					.AddTextRenderer (i => i.Name)
				.AddColumn("Инвентарный номер")
					.AddTextRenderer (i => i.InventoryNumber)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount)
					.WidthChars(10)
					.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.AddSetter((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnStock, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
				.AddColumn("Причина выбраковки")
					.AddComboRenderer(i => i.CullingCategory)
					.SetDisplayFunc(DomainHelper.GetTitle)
					.Editing()
					.FillItems(ViewModel.CullingCategories)
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф")
					.AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Выявлено в процессе")
					.AddTextRenderer(i => i.Comment)
					.Editable()
				.Finish();
			
			treeItemsList.ItemsDataSource = ViewModel.Entity.ObservableItems;
			treeItemsList.Binding
				.AddBinding(ViewModel, vm => vm.SelectedItem, w => w.SelectedRow)
				.InitializeFromSource();
			treeItemsList.RowActivated += OnTreeItemsListRowActivated;
		}

		#region ButtonHandlers

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}
		
		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Cancel);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}
		
		private void OnTreeItemsListRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.EditSelectedItemCommand.Execute();
		}
		
		private void OnAddNomenclatureClicked(object sender, EventArgs e)
		{
			ViewModel.AddNomenclatureCommand.Execute();
		}
		
		private void OnAddNomenclatureInstanceClicked(object sender, EventArgs e)
		{
			ViewModel.AddInventoryInstanceCommand.Execute();
		}
		
		private void OnDeleteItemClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteItemCommand.Execute();
		}
		
		private void OnAddFineClicked(object sender, EventArgs e)
		{
			ViewModel.AddOrEditFineCommand.Execute();
		}
		
		private void OnDeleteFineClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineCommand.Execute();
		}

		#endregion
	}
}
