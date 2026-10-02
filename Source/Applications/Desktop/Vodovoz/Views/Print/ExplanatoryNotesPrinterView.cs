using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Navigation;
using QS.Print;
using QS.Report;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Print.Receipts;

namespace Vodovoz.Views.Print
{
	public partial class ExplanatoryNotesPrinterView : TabViewBase<ExplanatoryNotesPrinterViewModel>
	{
		public ExplanatoryNotesPrinterView(ExplanatoryNotesPrinterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ConfigureTree();

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			ybtnPrintAll.Clicked += (sender, args) => ViewModel.PrintSelected();

			ViewModel.PreviewDocument += PreviewDocument;
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			if(ViewModel.SelectedDocument != null)
			{
				PreviewDocument();
			}
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.PrintableDocuments))
			{
				ConfigureTree();
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

			ytreeviewDocuments.ItemsDataSource = ViewModel.PrintableDocuments;
			ytreeviewDocuments.RowActivated += YTreeViewDocumentsOnRowActivated;
		}

		private void YTreeViewDocumentsOnRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.SelectedDocument = ytreeviewDocuments.GetSelectedObject<SelectablePrintDocument>();
		}

		private void PreviewDocument()
		{
			if(!(ViewModel.SelectedDocument?.Document is IPrintableRDLDocument rdldoc))
			{
				return;
			}

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

		public override void Destroy()
		{
			ViewModel.PreviewDocument -= PreviewDocument;
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.Destroy();
		}
	}
}
