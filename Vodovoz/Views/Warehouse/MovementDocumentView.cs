using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Documents;
using Gamma.ColumnConfig;

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
			ylabelAuthorValue.Binding.AddBinding(ViewModel.Entity, e => e.Author, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();
			ylabelCreationDateValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.TimeStamp.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelEditorValue.Binding.AddBinding(ViewModel.Entity, e => e.LastEditor, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();
			ylabelEditTimeValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.LastEditor != null ? e.LastEditedTime.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelSenderValue.Binding.AddBinding(ViewModel.Entity, e => e.Sender, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();
			ylabelSendTimeValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.SendTime.HasValue ? e.SendTime.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelReceiverValue.Binding.AddBinding(ViewModel.Entity, e => e.Receiver, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();
			ylabelReceiveTimeValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.ReceiveTime.HasValue ? e.ReceiveTime.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelDiscrepancyAccepterValue.Binding.AddBinding(ViewModel.Entity, e => e.DiscrepancyAccepter, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();
			ylabelDiscrepancyAcceptTimeValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.DiscrepancyAcceptTime.HasValue ? e.DiscrepancyAcceptTime.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			yentryrefWagon.SubjectType = typeof(MovementWagon);
			yentryrefWagon.Binding.AddBinding(ViewModel.Entity, e => e.MovementWagon, w => w.Subject).InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			comboWarehouseFrom.ItemsList = ViewModel.AllowedWarehousesFrom;
			comboWarehouseFrom.Binding.AddBinding(ViewModel.Entity, e => e.FromWarehouse, w => w.SelectedItem).InitializeFromSource();


			comboWarehouseTo.ItemsList = ViewModel.AllowedWarehousesTo;
			comboWarehouseTo.Binding.AddBinding(ViewModel.Entity, e => e.ToWarehouse, w => w.SelectedItem).InitializeFromSource();

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<MovementDocumentItem>.Create()
					.AddColumn("Наименование").AddTextRenderer(i => i.Name)
					.AddColumn("Отправлено")
						.AddNumericRenderer(i => i.SendedAmount)
						.AddSetter((c, i) => c.Editable = ViewModel.CanEditSendedAmount)
						.WidthChars(10)
						.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
						.AddSetter((c, i) => c.Adjustment = new Gtk.Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
						.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.AddColumn("Принято")
						.AddNumericRenderer(i => i.ReceivedAmount)
						.AddSetter((c, i) => c.Editable = ViewModel.CanEditReceivedAmount)
						.WidthChars(10)
						.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
						.AddSetter((c, i) => c.Adjustment = new Gtk.Adjustment(0, 0, (double)i.SendedAmount, 1, 100, 0))
						.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.AddColumn("")
					.Finish();

			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;

			ybuttonAddItem.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();
			ViewModel.AddItemCommand.CanExecuteChanged += (sender, e) => ybuttonAddItem.Sensitive = ViewModel.AddItemCommand.CanExecute();
			ybuttonAddItem.Sensitive = ViewModel.AddItemCommand.CanExecute();

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

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true);
		}

		private MovementDocumentItem GetSelectedItem()
		{
			return ytreeviewItems.GetSelectedObject() as MovementDocumentItem;
		}
	}
}
