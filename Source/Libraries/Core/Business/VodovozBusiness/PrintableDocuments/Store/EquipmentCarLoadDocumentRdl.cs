using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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
	public class EquipmentCarLoadDocumentRdl : PropertyChangedBase, ICustomPrinterPrintDocument, IDisposable
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserSettingsService _userSettingsService;
		private readonly CarLoadDocument _carLoadDocument;

		private int _copiesToPrint;
		private string _printerName;

		public EquipmentCarLoadDocumentRdl(
			IUnitOfWorkFactory unitOfWorkFactory,
			IUserSettingsService userSettingsService,
			CarLoadDocument carLoadDocument)
		{
			_unitOfWork =
				(unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot();
			_userSettingsService =
				userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_carLoadDocument =
				carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));

			SetPrinterSettings();
		}

		private void SetPrinterSettings()
		{
			var savedPrinterSettings =
				_userSettingsService.Settings.DocumentPrinterSettings
				.Where(s => s.DocumentType == DocumentType)
				.FirstOrDefault();

			if(savedPrinterSettings is null)
			{
				CopiesToPrint = 1;
				return;
			}

			CopiesToPrint = savedPrinterSettings.NumberOfCopies;
			PrinterName = savedPrinterSettings.PrinterName;
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

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
