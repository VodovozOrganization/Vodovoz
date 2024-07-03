using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Extensions;

namespace Vodovoz.PrintableDocuments.Store
{
	public class EquipmentCarLoadDocumentRdl : ICustomPrinterPrintDocument
	{
		private readonly CarLoadDocument _carLoadDocument;

		public EquipmentCarLoadDocumentRdl(CarLoadDocument carLoadDocument)
		{
			_carLoadDocument = carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
		}

		public Dictionary<object, object> Parameters { get; set; }
		public PrinterType PrintType => PrinterType.RDL;
		public DocumentOrientation Orientation => DocumentOrientation.Portrait;
		public int CopiesToPrint { get; set; } = 1;

		public string Name => DocumentType.GetEnumDisplayName();

		public CustomPrinterPrintDocumentType DocumentType => CustomPrinterPrintDocumentType.EquipmentCarLoadDocument;

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo
			{
				Title = _carLoadDocument.Title,
				Identifier = "Store.CarLoadDocumentEquipmentStore",
				Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } },
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};
		}
	}
}
