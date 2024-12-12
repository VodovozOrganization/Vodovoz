using System;
using Vodovoz.PrintableDocuments;

namespace Vodovoz.ViewModels.Infrastructure.Print
{
	public interface ICustomPrintRdlDocumentsPrinter
	{
		event EventHandler DocumentsPrinted;
		event EventHandler PrintingCanceled;
		void Print(ICustomPrintRdlDocument document);
	}
}
