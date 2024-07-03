using QS.Report;

namespace Vodovoz.PrintableDocuments
{
	public interface ICustomPrinterPrintDocument : IPrintableRDLDocument
	{
		CustomPrinterPrintDocumentType DocumentType { get; }
	}
}
