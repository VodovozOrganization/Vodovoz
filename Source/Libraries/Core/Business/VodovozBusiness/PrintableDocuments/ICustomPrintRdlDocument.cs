using QS.Report;
using Vodovoz.Core.Domain.PrintableDocuments;

namespace Vodovoz.PrintableDocuments
{
	public interface ICustomPrintRdlDocument : IPrintableRDLDocument
	{
		CustomPrintDocumentType DocumentType { get; }
		string PrinterName { get; set; }
	}
}
