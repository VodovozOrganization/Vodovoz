using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Extensions;
using Vodovoz.Services;

namespace Vodovoz.PrintableDocuments.Store
{
	public class EquipmentCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		private readonly CarLoadDocument _carLoadDocument;

		private int _copiesToPrint;
		private string _printerName;

		private EquipmentCarLoadDocumentRdl(CarLoadDocument carLoadDocument)
		{
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
			return new ReportInfo
			{
				Title = _carLoadDocument.Title,
				Identifier = "Store.CarLoadDocumentEquipmentStore",
				Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } },
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};
		}

		public static EquipmentCarLoadDocumentRdl Create(
			IUserSettingsService userSettingsService,
			CarLoadDocument carLoadDocument)
		{
			var document = new EquipmentCarLoadDocumentRdl(carLoadDocument);

			var savedPrinterSettings =
				userSettingsService.Settings.DocumentPrinterSettings
				.Where(s => s.DocumentType == document.DocumentType)
				.FirstOrDefault();

			document.CopiesToPrint = savedPrinterSettings is null ? 1 : savedPrinterSettings.NumberOfCopies;
			document.PrinterName = savedPrinterSettings?.PrinterName;

			return document;
		}
	}
}
