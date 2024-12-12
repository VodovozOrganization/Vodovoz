using System;
using System.Collections.Generic;
using QS.Print;

namespace Vodovoz.ViewModels.Infrastructure.Print
{
    public interface IEntityDocumentsPrinter
    {
        IList<SelectablePrintDocument> MultiDocPrinterPrintableDocuments { get; }
        string ODTTemplateNotFoundMessages { get; set; }
        event EventHandler DocumentsPrinted;
        event EventHandler PrintingCanceled;
        void Print(SelectablePrintDocument document = null);
    }
}
