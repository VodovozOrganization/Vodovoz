using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.PrintableDocuments;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Documents;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;

namespace Vodovoz.ViewModels.Print.Store
{
	public class EquipmentCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly CarLoadDocument _carLoadDocument;

		private int _copiesToPrint;
		private string _printerName;

		private EquipmentCarLoadDocumentRdl(IReportInfoFactory reportInfoFactory, CarLoadDocument carLoadDocument)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_carLoadDocument =
				carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
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

		public CustomPrintDocumentType DocumentType => CustomPrintDocumentType.EquipmentCarLoadDocument;

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Identifier = "Store.CarLoadDocumentEquipmentStore";
			reportInfo.Title = _carLoadDocument.Title;
			reportInfo.Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } };
			reportInfo.PrintType = ReportInfo.PrintingType.MultiplePrinters;
			return reportInfo;
		}

		public static EquipmentCarLoadDocumentRdl Create(
			UserSettings userSettings,
			CarLoadDocument carLoadDocument,
			IReportInfoFactory reportInfoFactory
			)
		{
			var document = new EquipmentCarLoadDocumentRdl(reportInfoFactory, carLoadDocument);

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
