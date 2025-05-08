using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;

namespace Vodovoz.Presentation.ViewModels.Documents
{
	public class PrintableRdlDocumentViewModel<TDocument> : RdlViewerViewModel
		where TDocument : class, IPrintableRDLDocument
	{
		public PrintableRdlDocumentViewModel(IPrintableRDLDocument document, INavigationManager navigation)
			: base(document.GetReportInfo(), navigation)
		{
		}
	}
}
