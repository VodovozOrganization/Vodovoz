using Gamma.ColumnConfig;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Print;

namespace Vodovoz.Views.Print
{
	[WindowSize(400, 400)]
	public partial class PrinterSelectionView : DialogViewBase<PrinterSelectionViewModel>
	{
		public PrinterSelectionView(PrinterSelectionViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelHeader.Binding
				.AddBinding(ViewModel, vm => vm.DialogHeader, w => w.Text)
				.InitializeFromSource();

			yspinbuttonNumberOfCopies.Binding
				.AddBinding(ViewModel, vm => vm.NumberOfCopies, w => w.ValueAsInt)
				.InitializeFromSource();

			ytreeviewPrinters.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("Принтер").HeaderAlignment(0.5f).AddTextRenderer(x => x).XAlign(0f)
				.AddColumn("")
				.Finish();

			ytreeviewPrinters.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Printers, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedPrinter, w => w.SelectedRow)
				.InitializeFromSource();

			ybuttonSelect.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectPrinter, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSelect.BindCommand(ViewModel.SelectPrinterCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
