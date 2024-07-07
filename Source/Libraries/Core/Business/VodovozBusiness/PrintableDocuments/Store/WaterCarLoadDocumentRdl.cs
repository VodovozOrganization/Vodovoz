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
	public class WaterCarLoadDocumentRdl : PropertyChangedBase, ICustomPrinterPrintDocument, IDisposable
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocument.rdl";

		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserSettingsService _userSettingsService;
		private readonly CarLoadDocument _carLoadDocument;
		private readonly ReportInfo _reportInfo;

		private int _copiesToPrint;
		private string _printerName;

		public WaterCarLoadDocumentRdl(
			IUnitOfWorkFactory unitOfWorkFactory,
			IUserSettingsService userSettingsService,
			CarLoadDocument carLoadDocument,
			ReportInfo reportInfo)
		{
			_unitOfWork =
				(unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot();
			_userSettingsService =
				userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_carLoadDocument =
				carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_reportInfo =
				reportInfo ?? throw new ArgumentNullException(nameof(reportInfo));

			SetPrinterSettings();
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

		public CustomPrinterPrintDocumentType DocumentType => CustomPrinterPrintDocumentType.WaterCarLoadDocument;

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

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			return _reportInfo;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
