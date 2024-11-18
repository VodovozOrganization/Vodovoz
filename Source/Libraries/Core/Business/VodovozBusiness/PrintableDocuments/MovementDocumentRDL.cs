using System.Collections.Generic;
using Autofac;
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
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = "Documents.MovementOperationDocucment";
			reportInfo.Title = Title;
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "documentId", Document.Id },
				{ "date", Document.TimeStamp.ToString("dd/MM/yyyy") }
			};
			return reportInfo;
		}
		public MovementDocumentRdl(MovementDocument document) => Document = document;
	}
}
