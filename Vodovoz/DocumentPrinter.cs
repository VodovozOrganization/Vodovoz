using System;
using Vodovoz.Domain.Orders.Documents;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz
{
	public class DocumentPrinter
	{		
		public static void Print(IPrintableDocument document)
		{
			throw new NotImplementedException (); // TODO напечатать документ не открывая ничего лишнего
		}

		public static void PrintAll(IEnumerable<IPrintableDocument> documents)
		{
			var i = documents.GetEnumerator ();
			for (i.Reset (); i.MoveNext();) {
				Print (i.Current);
			}
		}
		public static QSTDI.TdiTabBase PreviewTab(IPrintableDocument document)
		{
			return new QSReport.ReportViewDlg (document.GetReportInfo());				
		}
	}

	public interface IPrintableDocument
	{
		PrinterType PrintType{ get; }
		QSReport.ReportInfo GetReportInfo ();
	}

	public enum PrinterType{
		None, RDL
	}
}

