using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;

namespace Vodovoz.PrintableDocuments.Store
{
	public class WaterCarLoadDocumentRdl : IPrintableRDLDocument
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocument.rdl";

		private readonly CarLoadDocument _carLoadDocument;
		private readonly ReportInfo _reportInfo;

		public WaterCarLoadDocumentRdl(CarLoadDocument carLoadDocument, ReportInfo reportInfo)
		{
			_carLoadDocument = carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_reportInfo = reportInfo ?? throw new ArgumentNullException(nameof(reportInfo));
		}

		public Dictionary<object, object> Parameters { get; set; }
		public PrinterType PrintType => PrinterType.RDL;
		public DocumentOrientation Orientation => DocumentOrientation.Portrait;
		public int CopiesToPrint { get; set; } = 1;

		public string Name => "Талон погрузки (склад воды)";

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return _reportInfo;
		}
	}
}
