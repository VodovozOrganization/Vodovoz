using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Dialogs.Orders;
using static Vodovoz.ViewModels.Dialogs.Orders.PrintOrdersDocumentsViewModel;

namespace Vodovoz.Views.Orders
{
	public partial class PrintOrdersDocumentsView : TabViewBase<PrintOrdersDocumentsViewModel>
	{
		private readonly Adjustment _progressBarAdjustment =
			new Adjustment(0, 0, 0, 1, 1, 1);

		public PrintOrdersDocumentsView(PrintOrdersDocumentsViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonPrint.Clicked += (_, _2) => ViewModel.PrintCommand.Execute();
			ybuttonPrint.Binding
				.AddBinding(ViewModel, vm => vm.CanPrintOrSaveDocuments, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();
			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanPrintOrSaveDocuments, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCancel.Clicked += (_, _2) => ViewModel.CloseDialogCommand.Execute();

			ybuttonSelectAll.Clicked += (_, _2) => ViewModel.SelectAllOrdersCommand.Execute();

			ybuttonDeselectAll.Clicked += (_, _2) => ViewModel.DeselectAllOrdersCommand.Execute();

			ycheckbuttonBill.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintBill, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonBillWithSignatureAndStamp.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintBillWithSignatureAndStamp, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonUpd.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintUpd, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonUpdWithSignatureAndStamp.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintUpdWithSignatureAndStamp, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonSpecialBill.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintSpecialBill, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonSpecialBillWithSignatureAndStamp.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintSpecialBillWithSignatureAndStamp, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonSpecialUpd.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintSpecialUpd, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonSpecialUpdWithSignatureAndStamp.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintSpecialUpdWithSignatureAndStamp, w => w.Active)
				.InitializeFromSource();

			yspinbuttonCopiesCount.Binding
				.AddBinding(ViewModel, vm => vm.PrintCopiesCount, w => w.ValueAsInt)
				.InitializeFromSource();

			ytreeviewOrders.ColumnsConfig = ColumnsConfigFactory.Create<OrdersToPrintNode>()
				.AddColumn("Заказ").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата").AddTextRenderer(o => $"{o.DeliveryDate:dd.MM.yyyy}")
				.AddColumn("Выбрано").AddToggleRenderer(x => x.Selected)
				.AddColumn("")
				.Finish();

			ytreeviewOrders.ItemsDataSource = ViewModel.OrdersToPrint;

			ytreeviewWarnings.HeadersVisible = false;
			ytreeviewWarnings.ColumnsConfig = ColumnsConfigFactory.Create<string>()
				.AddColumn("")
					.AddTextRenderer(x => x)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = GdkColors.DangerText)
				.Finish();

			ytreeviewWarnings.ItemsDataSource = ViewModel.Warnings;

			yhboxWarnings.Binding
				.AddBinding(ViewModel, vm => vm.IsShowWarnings, w => w.Visible)
				.InitializeFromSource();

			yprogressbar.Adjustment = _progressBarAdjustment;

			yprogressbar.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedToPrintCount, w => w.Adjustment.Upper)
				.AddBinding(vm => vm.PrintingDocumentInfo, w => w.Text)
				.AddBinding(vm => vm.OrdersPrintedCount, w => w.Adjustment.Value)
				.InitializeFromSource();
		}

		public override void Destroy()
		{
			ytreeviewOrders?.Destroy();
			ytreeviewWarnings?.Destroy();

			base.Destroy();
		}
	}
}
