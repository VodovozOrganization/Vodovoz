using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Documents;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Journals;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.Views.Warehouse
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MovementDocumentView : TabViewBase<MovementDocumentViewModel>
	{
		public MovementDocumentView(MovementDocumentViewModel viewModel) : base(viewModel)
		{
			this.Build();
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

			//yentryrefWagon.SubjectType = typeof(MovementWagon);
			yentryrefWagon.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<MovementWagon, MovementWagonJournalViewModel, MovementWagonJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
				);
			yentryrefWagon.Binding.AddBinding(ViewModel.Entity, e => e.MovementWagon, w => w.Subject).InitializeFromSource();

			yentryrefWagon.CanEditReference = false;
			yentryrefWagon.Binding.AddBinding(ViewModel, vm => vm.CanChangeWagon, w => w.Sensitive).InitializeFromSource();
			ylabelWagon.Binding.AddBinding(ViewModel, vm => vm.CanVisibleWagon, w => w.Visible).InitializeFromSource();
			yentryrefWagon.Binding.AddBinding(ViewModel, vm => vm.CanVisibleWagon, w => w.Visible).InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEditNewDocument, w => w.Editable).InitializeFromSource();

			comboWarehouseFrom.Binding.AddBinding(ViewModel, vm => vm.WarehousesFrom, w => w.ItemsList).InitializeFromSource();
			comboWarehouseFrom.Binding.AddBinding(ViewModel.Entity, e => e.FromWarehouse, w => w.SelectedItem).InitializeFromSource();
			comboWarehouseFrom.Binding.AddBinding(ViewModel, vm => vm.CanChangeWarehouseFrom, w => w.Sensitive).InitializeFromSource();

			comboWarehouseTo.Binding.AddBinding(ViewModel, vm => vm.WarehousesTo, w => w.ItemsList).InitializeFromSource();
			comboWarehouseTo.Binding.AddBinding(ViewModel.Entity, e => e.ToWarehouse, w => w.SelectedItem).InitializeFromSource();
			comboWarehouseTo.Binding.AddBinding(ViewModel, vm => vm.CanEditNewDocument, w => w.Sensitive).InitializeFromSource();

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<MovementDocumentItem>.Create()
					.AddColumn("Наименование").HeaderAlignment(0.5f)
						.AddTextRenderer(i => i.Name)
					.AddColumn("Отправлено").HeaderAlignment(0.5f)
						.AddNumericRenderer(i => i.SendedAmount, false)
						.XAlign(0.5f)
						.AddSetter((c, i) => c.Editable = ViewModel.CanEditSendedAmount)
						.WidthChars(10)
						.AddSetter((c, i) => c.Adjustment = new Gtk.Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
						.AddSetter((c, i) => c.Digits = (uint)(i.Nomenclature?.Unit?.Digits ?? 0))
						.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.AddColumn("Принято").HeaderAlignment(0.5f)
						.AddNumericRenderer(i => i.ReceivedAmount)
						.XAlign(0.5f)
						.AddSetter((c, i) => c.Editable = ViewModel.CanEditReceivedAmount)
						.WidthChars(10)
						.AddSetter((c, i) => c.Adjustment = new Gtk.Adjustment(0, 0, 99999999, 1, 100, 0))
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

		private MovementDocumentItem GetSelectedItem()
		{
			return ytreeviewItems.GetSelectedObject() as MovementDocumentItem;
		}
	}
}
