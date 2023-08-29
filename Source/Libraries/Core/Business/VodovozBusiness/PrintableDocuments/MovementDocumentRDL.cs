using System.Collections.Generic;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;

namespace Vodovoz.PrintableDocuments
{
	public class MovementDocumentRdl : IPrintableRDLDocument
	{
		public string Title { get; set; } = "Документ перемещения";

		public MovementDocument Document { get; set; }

		public Dictionary<object, object> Parameters { get; set; }

		public PrinterType PrintType { get; set; } = PrinterType.RDL;

		public DocumentOrientation Orientation { get; set; } = DocumentOrientation.Portrait;

		public int CopiesToPrint { get; set; } = 1;

		public string Name { get; set; } = "Документ перемещения";

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo {
				Title = Document.Title,
				Identifier = "Documents.MovementOperationDocucment",
				Parameters = new Dictionary<string, object>
				{
					{ "documentId" , Document.Id} ,
					{ "date" , Document.TimeStamp.ToString("dd/MM/yyyy")}
				}
			};
		}
		public MovementDocumentRdl(MovementDocument document) => Document = document;
	}
}
