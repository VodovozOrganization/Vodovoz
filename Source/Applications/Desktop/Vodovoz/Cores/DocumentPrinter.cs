using System.Collections.Generic;
using QS.DocTemplates;
using QS.Print;
using QSDocTemplates;
using Vodovoz.Infrastructure.Print;

namespace Vodovoz.Core
{
	public class DocumentPrinter : IDocumentPrinter
	{
		public void PrintAllDocuments(IEnumerable<IPrintableDocument> documents)
			=> new QSReport.DocumentPrinter().PrintAll(documents);

		public void PrintAllODTDocuments(IList<IPrintableOdtDocument> documents)
			=> TemplatePrinter.PrintAll(documents);
	}
}