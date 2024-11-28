using Gamma.ColumnConfig;
using Gtk;
using QS.Report;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Print;
using static Vodovoz.ViewModels.Print.PrintDocumentsSelectablePrinterViewModel;

namespace Vodovoz.Views.Print
{
	public partial class PrintDocumentsSelectablePrinterView : TabViewBase<PrintDocumentsSelectablePrinterViewModel>
	{
		public PrintDocumentsSelectablePrinterView(PrintDocumentsSelectablePrinterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ConfigureTree();

			ybtnPrintSelected.BindCommand(ViewModel.PrintSelectedDocumentsCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
			ybuttonSetPrinterSettings.BindCommand(ViewModel.EditPrintrSettingsCommand);
			ybuttonSavePrinterSettings.BindCommand(ViewModel.SavePrinterSettingsCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.DocumentsNodes))
			{
				ViewModel.PreviewDocument -= PreviewDocument;
				if(ViewModel.Printer != null)
				{
					ConfigureTree();
					ViewModel.PreviewDocument += PreviewDocument;
				}
			}
		}

		private void ConfigureTree()
		{
			ytreeviewDocuments.RowActivated -= YTreeViewDocumentsOnRowActivated;
			ytreeviewDocuments.ColumnsConfig = FluentColumnsConfig<PrintDocumentSelectableNode>.Create()
				.AddColumn("✓").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Документ").AddEnumRenderer(x => x.DocumentType)
				.AddColumn("Принтер").AddTextRenderer(x => x.PrinterName)
				.AddColumn("Копий").AddNumericRenderer(x => x.NumberOfCopies)
				.AddColumn("")
				.RowCells()
				.Finish();

			ytreeviewDocuments.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ActiveNode, w => w.SelectedRow)
				.AddBinding(vm => vm.DocumentsNodes, w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeviewDocuments.RowActivated += YTreeViewDocumentsOnRowActivated;
		}

		private void YTreeViewDocumentsOnRowActivated(object o, RowActivatedArgs args)
		{
			PreviewDocument();
		}

		private void PreviewDocument()
		{
			if(ViewModel.ActiveDocument is IPrintableRDLDocument rdldoc)
			{
				reportviewer.ReportPrinted -= ReportViewerOnReportPrinted;
				reportviewer.ReportPrinted += ReportViewerOnReportPrinted;
				var reportInfo = rdldoc.GetReportInfo();

				if(reportInfo.Source != null)
				{
					reportviewer.LoadReport(
						reportInfo.Source,
						reportInfo.GetParametersString(),
						reportInfo.ConnectionString,
						true,
						reportInfo.RestrictedOutputPresentationTypes);
				}
				else
				{
					reportviewer.LoadReport(
						reportInfo.GetReportUri(),
						reportInfo.GetParametersString(),
						reportInfo.ConnectionString,
						true,
						reportInfo.RestrictedOutputPresentationTypes);
				}
			}
		}

		private void ReportViewerOnReportPrinted(object sender, EventArgs e) => ViewModel.ReportPrintedCommand.Execute();

		public override void Destroy()
		{
			if(reportviewer != null)
			{
				reportviewer.ReportPrinted -= ReportViewerOnReportPrinted;
			}

			ViewModel.PreviewDocument -= PreviewDocument;
			base.Destroy();
		}
	}
}
