﻿using Gamma.ColumnConfig;
using Gtk;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class IncomingInvoiceView : TabViewBase<IncomingInvoiceViewModel>
	{
		public IncomingInvoiceView(IncomingInvoiceViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			#region Bindings
			
			ylabelSum.Binding.AddBinding(ViewModel, vm => vm.TotalSum, w => w.LabelProp).InitializeFromSource();
			
			ybtnAdd.Clicked += (sender, args) => ViewModel.AddItemCommand.Execute();
			buttonDelete.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute();
			ViewModel.DeleteItemCommand.CanExecuteChanged +=
				(sender, e) => buttonDelete.Sensitive = ViewModel.DeleteItemCommand.CanExecute();

			ybtnAddFromOrders.Clicked += (sender, e) => ViewModel.FillFromOrdersCommand.Execute();
			ViewModel.FillFromOrdersCommand.CanExecuteChanged += (sender, e) => ybtnAddFromOrders.Sensitive = ViewModel.FillFromOrdersCommand.CanExecute();
			ybtnAddFromOrders.Sensitive = ViewModel.FillFromOrdersCommand.CanExecute();
			
			treeItemsList.Selection.Changed += (sender, args) => { buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows() > 0; };
			
			labelTimeStamp.Binding.AddBinding(ViewModel.Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();
			entryInvoiceNumber.Binding.AddBinding(ViewModel.Entity, e => e.InvoiceNumber, w => w.Text).InitializeFromSource();
			entryWaybillNumber.Binding.AddBinding(ViewModel.Entity, e => e.WaybillNumber, w => w.Text).InitializeFromSource();
			
			lstWarehouse.SetRenderTextFunc<Domain.Store.Warehouse>(w => w.Name);
			lstWarehouse.Binding.AddBinding(ViewModel, vm => vm.Warehouses, w => w.ItemsList).InitializeFromSource();
			lstWarehouse.Binding.AddBinding(ViewModel.Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			
			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityVMEntryClient.Binding.AddBinding(ViewModel.Entity, s => s.Contractor, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = !ViewModel.UserHasOnlyAccessToWarehouseAndComplaints;
			
			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			
			btnPrint.Clicked += (sender, e) => ViewModel.PrintCommand.Execute();
			
			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive);
			
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
			btnAddInventoryInstance.Clicked += (sender, args) => ViewModel.AddInventoryInstanceCommand.Execute();
			btnAddInventoryInstance.Binding
				.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive)
				.InitializeFromSource();
			btnCopyInventoryInstance.Clicked += (sender, args) => ViewModel.CopyInventoryInstanceCommand.Execute();
			btnCopyInventoryInstance.Binding
				.AddBinding(ViewModel, vm => vm.CanDuplicateInstance, w => w.Sensitive)
				.InitializeFromSource();
			
			#endregion
			
			#region Таблица

			treeItemsList.ColumnsConfig =  FluentColumnsConfig<IncomingInvoiceItem>.Create()
				.AddColumn("№ п/п")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => (i.Document.Items.IndexOf(i)+ 1).ToString())
				.AddColumn("Наименование")
					.HeaderAlignment(0.5f)
					.AddTextRenderer (i => i.Name)
				.AddColumn("Инвентарный\nномер")
					.HeaderAlignment(0.5f)
					.AddTextRenderer (i => i.InventoryNumberString)
					.XAlign(0.5f)
				.AddColumn("% НДС")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer (i => i.VAT)
					.Editing()
				.AddColumn("Количество")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.Amount)
					.WidthChars(10)
					.AddSetter((c, i) =>
						c.Digits = i.Nomenclature.Unit == null ? 1 :(uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.Adjustment(new Adjustment (0, 0, 1000000, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit == null ? string.Empty: i.Nomenclature.Unit.Name, false)
				.AddColumn("Цена\nзакупки")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.PrimeCost)
					.Digits(2)
					.Editing()
					.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0))
					.AddTextRenderer(i => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => CurrencyWorks.GetShortCurrencyString(i.Sum))
				.AddColumn("")
				.Finish();
			
			treeItemsList.ItemsDataSource = ViewModel.Entity.ObservableItems;
			treeItemsList.Binding
				.AddBinding(ViewModel, vm => vm.SelectedItem, w => w.SelectedRow)
				.InitializeFromSource();

			#endregion
		}
	}
}
