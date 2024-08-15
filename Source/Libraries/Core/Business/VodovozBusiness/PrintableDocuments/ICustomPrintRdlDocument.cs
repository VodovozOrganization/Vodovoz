using QS.Report;

namespace Vodovoz.PrintableDocuments
{
	public interface ICustomPrintRdlDocument : IPrintableRDLDocument
	{
		CustomPrintDocumentType DocumentType { get; }
		string PrinterName { get; set; }
	}
}
