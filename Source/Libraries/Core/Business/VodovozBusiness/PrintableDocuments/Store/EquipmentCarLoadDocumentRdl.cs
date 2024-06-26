using QS.Print;
using QS.Report;
using System.Collections.Generic;
using System;
using Vodovoz.Domain.Documents;

namespace Vodovoz.PrintableDocuments.Store
{
	public class EquipmentCarLoadDocumentRdl : IPrintableRDLDocument
	{
		private readonly CarLoadDocument _carLoadDocument;
		private readonly ReportInfo _reportInfo;

		public EquipmentCarLoadDocumentRdl(CarLoadDocument carLoadDocument, ReportInfo reportInfo)
		{
			_carLoadDocument = carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_reportInfo = reportInfo ?? throw new ArgumentNullException(nameof(reportInfo));
		}

		public Dictionary<object, object> Parameters { get; set; }
		public PrinterType PrintType => PrinterType.RDL;
		public DocumentOrientation Orientation => DocumentOrientation.Portrait;
		public int CopiesToPrint { get; set; } = 1;

		public string Name => "Талон погрузки (склад оборудования)";

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return _reportInfo;
		}
	}
}
