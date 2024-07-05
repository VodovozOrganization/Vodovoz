using Gamma.ColumnConfig;
using Gtk;
using QS.Print;
using QS.Report;
using QS.Views.Dialog;
using System;
using Vodovoz.ViewModels.Print;
namespace Vodovoz.Views.Print
{
	public partial class PrintDocumentsSelectablePrinterView : DialogViewBase<PrintDocumentsSelectablePrinterViewModel>
	{
		public PrintDocumentsSelectablePrinterView(PrintDocumentsSelectablePrinterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ConfigureTree();

			ybtnPrintSelected.BindCommand(ViewModel.PrintSelectedCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			if(ViewModel.EntityDocumentsPrinter != null)
			{
				ViewModel.PreviewDocument += PreviewDocument;
			}
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.EntityDocumentsPrinter))
			{
				ViewModel.PreviewDocument -= PreviewDocument;
				if(ViewModel.EntityDocumentsPrinter != null)
				{
					ConfigureTree();
					ViewModel.PreviewDocument += PreviewDocument;
				}
			}
		}

		private void ConfigureTree()
		{
			ytreeviewDocuments.RowActivated -= YTreeViewDocumentsOnRowActivated;
			ytreeviewDocuments.ColumnsConfig = FluentColumnsConfig<SelectablePrintDocument>.Create()
				.AddColumn("✓")
					.AddToggleRenderer(x => x.Selected)
				.AddColumn("Документ")
					.AddTextRenderer(x => x.Document.Name)
				.AddColumn("Копий")
					.AddNumericRenderer(x => x.Copies)
					.Editing()
					.Adjustment(new Adjustment(0, 0, 10000, 1, 100, 0))
				.AddColumn("Принтер")
				.AddColumn("Ориентация")
				.AddColumn("")
				.RowCells()
				.Finish();

			if(ViewModel.EntityDocumentsPrinter != null)
			{
				ytreeviewDocuments.ItemsDataSource = ViewModel.EntityDocumentsPrinter.MultiDocPrinterPrintableDocuments;
			}
			ytreeviewDocuments.RowActivated += YTreeViewDocumentsOnRowActivated;
		}

		private void YTreeViewDocumentsOnRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.SelectedDocument = ytreeviewDocuments.GetSelectedObject<SelectablePrintDocument>();
			PreviewDocument();
		}

		private void PreviewDocument()
		{
			if(ViewModel.SelectedDocument.Document is IPrintableRDLDocument rdldoc)
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
