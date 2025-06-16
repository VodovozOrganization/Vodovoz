using Vodovoz.Core.Domain.PrintableDocuments;

namespace Vodovoz.ViewModels.Print
{
	public partial class PrintDocumentsSelectablePrinterViewModel
	{
		public class PrintDocumentSelectableNode
		{
			public bool IsSelected { get; set; }
			public CustomPrintDocumentType DocumentType { get; set; }
			public string PrinterName { get; set; }
			public int NumberOfCopies { get; set; }
		}

	}
}
