using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Extensions;

namespace Vodovoz.PrintableDocuments.Store
{
	public class WaterCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocument.rdl";

		private readonly CarLoadDocument _carLoadDocument;
		private readonly ReportInfo _reportInfo;

		private int _copiesToPrint;
		private string _printerName;

		private WaterCarLoadDocumentRdl(
			CarLoadDocument carLoadDocument,
			ReportInfo reportInfo)
		{
			_carLoadDocument =
				carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_reportInfo =
				reportInfo ?? throw new ArgumentNullException(nameof(reportInfo));
		}

		public Dictionary<object, object> Parameters { get; set; }
		public PrinterType PrintType => PrinterType.RDL;
		public DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public int CopiesToPrint
		{
			get => _copiesToPrint;
			set => SetField(ref _copiesToPrint, value);
		}

		public string PrinterName
		{
			get => _printerName;
			set => SetField(ref _printerName, value);
		}

		public string Name => DocumentType.GetEnumDisplayName();

		public CustomPrintDocumentType DocumentType => CustomPrintDocumentType.WaterCarLoadDocument;

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return _reportInfo;
		}

		public static WaterCarLoadDocumentRdl Create(
			UserSettings userSettings,
			CarLoadDocument carLoadDocument,
			ReportInfo reportInfo)
		{
			var document = new WaterCarLoadDocumentRdl(carLoadDocument, reportInfo);

			var savedPrinterSettings =
				userSettings.DocumentPrinterSettings
				.Where(s => s.DocumentType == document.DocumentType)
				.FirstOrDefault();

			document.CopiesToPrint = savedPrinterSettings is null ? 1 : savedPrinterSettings.NumberOfCopies;
			document.PrinterName = savedPrinterSettings?.PrinterName;

			return document;
		}
	}
}
