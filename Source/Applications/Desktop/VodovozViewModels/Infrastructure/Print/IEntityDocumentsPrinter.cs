using System;
using System.Collections.Generic;
using QS.Print;

namespace Vodovoz.ViewModels.Infrastructure.Print
{
    public interface IEntityDocumentsPrinter
    {
        List<SelectablePrintDocument> DocumentsToPrint { get; set; }
        IList<SelectablePrintDocument> MultiDocPrinterPrintableDocuments { get; }
        string ODTTemplateNotFoundMessages { get; set; }
        event EventHandler DocumentsPrinted;
        event EventHandler PrintingCanceled;
        void Print(SelectablePrintDocument document = null);
    }
}