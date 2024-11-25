using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Navigation;
using QS.Print;
using QS.Report;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Orders;

namespace Vodovoz.Views.Print
{
    public partial class DocumentsPrinterView : TabViewBase<DocumentsPrinterViewModel>
	{
        public DocumentsPrinterView(DocumentsPrinterViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
            ConfigureTree();

            buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
            ybtnPrintAll.Clicked += (sender, args) => ViewModel.PrintAll();

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

        private void ReportViewerOnReportPrinted(object sender, EventArgs e) =>
            ViewModel.ReportViewerOnReportPrinted(this, new EndPrintArgs {Args = new object[] {ViewModel.SelectedDocument.Document}});

        public override void Destroy()
        {
	        ViewModel.PreviewDocument -= PreviewDocument;
	        base.Destroy();
        }
    }
}
