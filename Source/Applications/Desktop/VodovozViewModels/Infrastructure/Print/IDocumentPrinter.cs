using System.Collections.Generic;
using QS.DocTemplates;
using QS.Print;

namespace Vodovoz.Infrastructure.Print
{
	public interface IDocumentPrinter
	{
		void PrintAllDocuments(IEnumerable<IPrintableDocument> documents);
		void PrintAllODTDocuments(IList<IPrintableOdtDocument> documents);
	}
}