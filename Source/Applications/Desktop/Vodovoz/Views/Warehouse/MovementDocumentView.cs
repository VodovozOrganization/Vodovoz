﻿using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Domain.Documents;
using Gamma.ColumnConfig;
using Gamma.Utilities;

namespace Vodovoz.Views.Warehouse
{
	public partial class MovementDocumentView : TabViewBase<MovementDocumentViewModel>
	{
		public MovementDocumentView(MovementDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			ylabelStatusValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelAuthorValue.Binding.AddBinding(ViewModel, vm => vm.AuthorInfo, w => w.LabelProp).InitializeFromSource();
			ylabelEditorValue.Binding.AddBinding(ViewModel, vm => vm.LastEditorInfo, w => w.LabelProp).InitializeFromSource();
			ylabelSenderValue.Binding.AddBinding(ViewModel, vm => vm.SendedInfo, w => w.LabelProp).InitializeFromSource();
			ylabelReceiverValue.Binding.AddBinding(ViewModel, vm => vm.ReceiverInfo, w => w.LabelProp).InitializeFromSource();
			ylabelDiscrepancyAccepterValue.Binding.AddBinding(ViewModel, vm => vm.DiscrepancyAccepterInfo, w => w.LabelProp).InitializeFromSource();
			ylabelDiscrepancyAccepterValue.Binding.AddBinding(ViewModel.Entity, e => e.DiscrepancyAccepter, w => w.Visible, new NullToBooleanConverter()).InitializeFromSource();
			ylabelDiscrepancyAccepter.Binding.AddBinding(ViewModel.Entity, e => e.DiscrepancyAccepter, w => w.Visible, new NullToBooleanConverter()).InitializeFromSource();

			enumCmbMovementTypeByStorage.ItemsEnum = typeof(MovementDocumentTypeByStorage);
			enumCmbMovementTypeByStorage.Binding
				.AddBinding(ViewModel.Entity, e => e.MovementDocumentTypeByStorage, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeDocumentTypeByStorageAndStorageFrom, w => w.Sensitive)
				.InitializeFromSource();

			wagonEntry.WidthRequest = 350;
			wagonEntry.ViewModel = ViewModel.WagonEntryViewModel;
			//wagonEntry.ViewModel.IsEditable = false;
			wagonEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeWagon, w => w.Sensitive)
				.InitializeFromSource();
			ylabelWagon.Binding
				.AddBinding(ViewModel, vm => vm.CanVisibleWagon, w => w.Visible)
				.InitializeFromSource();
			wagonEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanVisibleWagon, w => w.Visible)
				.InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEditNewDocument, w => w.Editable).InitializeFromSource();

			#region Отправитель

			enumCmbStorage.ItemsEnum = typeof(Storage);
			enumCmbStorage.Binding
				.AddBinding(ViewModel.Entity, e => e.StorageFrom, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeDocumentTypeByStorageAndStorageFrom, w => w.Sensitive)
				.InitializeFromSource();

			vboxStorageFrom.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeDocumentTypeByStorageAndStorageFrom, w => w.Sensitive)
				.InitializeFromSource();
			hboxWarehouseFrom.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseFrom, w => w.Visible)
				.InitializeFromSource();
			comboWarehouseFrom.Binding
				.AddBinding(ViewModel, vm => vm.WarehousesFrom, w => w.ItemsList)
				.InitializeFromSource();
			comboWarehouseFrom.Binding
				.AddBinding(ViewModel.Entity, e => e.FromWarehouse, w => w.SelectedItem)
				.InitializeFromSource();

			employeeEntryFrom.ViewModel = ViewModel.FromEmployeeStorageEntryViewModel;
			employeeEntryFrom.Binding
				.AddBinding(ViewModel, vm => vm.CanShowEmployeeFrom, w => w.Visible)
				.InitializeFromSource();
			carEntryFrom.ViewModel = ViewModel.FromCarStorageEntryViewModel;
			carEntryFrom.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarFrom, w => w.Visible)
				.InitializeFromSource();

			#endregion

			#region Получатель

			hboxWarehouseTo.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseTo, w => w.Visible)
				.InitializeFromSource();
			comboWarehouseTo.Binding
				.AddBinding(ViewModel, vm => vm.WarehousesTo, w => w.ItemsList)
				.InitializeFromSource();
			comboWarehouseTo.Binding
				.AddBinding(ViewModel.Entity, e => e.ToWarehouse, w => w.SelectedItem)
				.InitializeFromSource();
			comboWarehouseTo.Binding
				.AddBinding(ViewModel, vm => vm.CanEditNewDocument, w => w.Sensitive)
				.InitializeFromSource();

			employeeEntryTo.ViewModel = ViewModel.ToEmployeeStorageEntryViewModel;
			employeeEntryTo.Binding
				.AddBinding(ViewModel, vm => vm.CanShowEmployeeTo, w => w.Visible)
				.InitializeFromSource();
			carEntryTo.ViewModel = ViewModel.ToCarStorageEntryViewModel;
			carEntryTo.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarTo, w => w.Visible)
				.InitializeFromSource();

			#endregion

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<MovementDocumentItem>.Create()
				.AddColumn("Наименование").HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.Name)
				.AddColumn("Инвентарный номер").HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.InventoryNumber)
				.AddColumn("Отправлено").HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.SentAmount, false)
					.XAlign(0.5f)
					.AddSetter((c, i) =>
						c.Editable = ViewModel.CanEditSentAmount && i.CanEditAmount)
					.WidthChars(10)
					.AddSetter((c, i) => c.Adjustment = new Gtk.Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
					.AddSetter((c, i) => c.Digits = (uint)(i.Nomenclature?.Unit?.Digits ?? 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
				.AddColumn("Принято").HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.ReceivedAmount)
					.XAlign(0.5f)
					.AddSetter((c, i) => c.Editable = ViewModel.CanEditReceivedAmount)
					.WidthChars(10)
					.AddSetter((c, i) =>
						{
							c.Adjustment = i.CanEditAmount
								? new Gtk.Adjustment(0, 0, 99999999, 1, 100, 0)
								: new Gtk.Adjustment(0, 0, 1, 1, 1, 0);
						})
					.AddSetter((c, i) => c.Digits = (uint)(i.Nomenclature?.Unit?.Digits ?? 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
				.AddColumn("")
				.Finish();

			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;

			ybuttonAddItem.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
			ViewModel.AddItemCommand.CanExecuteChanged += (sender, e) => ybuttonAddItem.Sensitive = ViewModel.AddItemCommand.CanExecute();
			ybuttonAddItem.Sensitive = ViewModel.AddItemCommand.CanExecute();

			ybuttonFillFromOrders.Clicked += (sender, e) => ViewModel.FillFromOrdersCommand.Execute();
			ViewModel.FillFromOrdersCommand.CanExecuteChanged += (sender, e) => ybuttonFillFromOrders.Sensitive = ViewModel.FillFromOrdersCommand.CanExecute(); 
			ybuttonFillFromOrders.Sensitive = ViewModel.FillFromOrdersCommand.CanExecute();

			ybuttonDeleteItem.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute(GetSelectedItem());
			ViewModel.DeleteItemCommand.CanExecuteChanged += (sender, e) => ybuttonDeleteItem.Sensitive = ViewModel.DeleteItemCommand.CanExecute(GetSelectedItem());
			ytreeviewItems.Selection.Changed += (sender, e) => ViewModel.DeleteItemCommand.RaiseCanExecuteChanged();
			ybuttonDeleteItem.Sensitive = ViewModel.DeleteItemCommand.CanExecute(GetSelectedItem());
			
			btnAddNomenclatureInstance.Clicked += OnAddNomenclatureInstanceClicked;
			btnAddNomenclatureInstance.Binding
				.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonSend.Clicked += (sender, e) => ViewModel.SendCommand.Execute();
			ViewModel.SendCommand.CanExecuteChanged += (sender, e) => buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();
			buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();

			buttonReceive.Clicked += (sender, e) => ViewModel.ReceiveCommand.Execute();
			ViewModel.ReceiveCommand.CanExecuteChanged += (sender, e) => buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();
			buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();

			buttonAcceptDiscrepancy.Clicked += (sender, e) => ViewModel.AcceptDiscrepancyCommand.Execute();
			ViewModel.AcceptDiscrepancyCommand.CanExecuteChanged += (sender, e) => buttonAcceptDiscrepancy.Sensitive = ViewModel.AcceptDiscrepancyCommand.CanExecute();
			buttonAcceptDiscrepancy.Sensitive = ViewModel.AcceptDiscrepancyCommand.CanExecute();

			buttonPrint.Clicked += (sender, e) => ViewModel.PrintCommand.Execute();
			ViewModel.PrintCommand.CanExecuteChanged += (sender, e) =>  buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute();
			buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute();

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		private void OnAddNomenclatureInstanceClicked(object sender, EventArgs e)
		{
			ViewModel.AddInventoryInstanceCommand.Execute();
		}

		private MovementDocumentItem GetSelectedItem()
		{
			return ytreeviewItems.GetSelectedObject() as MovementDocumentItem;
		}

		public override void Dispose()
		{
			btnAddNomenclatureInstance.Clicked -= OnAddNomenclatureInstanceClicked;
			base.Dispose();
		}
	}
}
