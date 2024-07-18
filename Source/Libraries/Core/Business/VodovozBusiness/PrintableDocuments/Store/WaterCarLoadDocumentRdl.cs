using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Extensions;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.PrintableDocuments.Store
{
	public class WaterCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocument.rdl";

		private const int _vol19LBottlesOnTrayCount = 48;
		private const int _vol6LBottlesOnTrayCount = 10;
		private const int _vol1500mlBottlesOnTrayCount = 30;
		private const int _vol500mlBottlesOnTrayCount = 60;
		private const int _traysOnPallet = 3;

		private readonly CarLoadDocument _carLoadDocument;
		private readonly Func<int, string, string> _qRPlacerFunc;
		private int _copiesToPrint;
		private string _printerName;

		private WaterCarLoadDocumentRdl(
			CarLoadDocument carLoadDocument,
			Func<int, string, string> qRPlacerFunc)
		{
			_carLoadDocument =
				carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_qRPlacerFunc =
				qRPlacerFunc ?? throw new ArgumentNullException(nameof(qRPlacerFunc));
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
			//Для каждой таблицы с сетевыми заказами добаляется Rectangle с названием "OrderQrRectangle_12345" (вместо 12345 номер заказа)
			//Предполагается, что в этот Rectangle будет в дальнейшем добавлен QR в котором содержится номер заказа
			//Когда будет добавлен QR заказа, установить значение  константы _dataTableWithQrHeaderRowHeightInPt = 125 (примерно) в классе WaterCarLoadDocumentModifier
			var source = _qRPlacerFunc.Invoke(_carLoadDocument.Id, GetReportSource());

			var reportInfo = new ReportInfo
			{
				Source = source,
				Title = Name,
				Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } },
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};

			return reportInfo;
		}

		private string GetReportSource()
		{
			return ModifyReport(DocumentRdlPath);
		}

		private string ModifyReport(string path)
		{
			var modifier = GetReportModifier();

			using(ReportController reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				reportController.AddModifier(modifier);
				reportController.Modify();
				reportController.Save(reportStream);

				using(var reader = new StreamReader(reportStream))
				{
					reportStream.Position = 0;
					var outputSource = reader.ReadToEnd();
					return outputSource;
				}
			}
		}

		private ReportModifierBase GetReportModifier()
		{
			var modifier = new WaterCarLoadDocumentModifier();
			var tearOffCouponsCount = GetTearOffCouponsCount();
			var isDocumentHasCommonOrders = IsDocumentHasCommonOrders();

			modifier.Setup(SeparateTableOrderIds, tearOffCouponsCount, isDocumentHasCommonOrders);

			return modifier;
		}

		private IEnumerable<int> SeparateTableOrderIds =>
			_carLoadDocument.Items
			.Where(item => item.OrderId.HasValue && item.IsIndividualSetForOrder)
			.Select(item => item.OrderId.Value)
			.Distinct()
			.ToList();

		private int GetTearOffCouponsCount()
		{
			var vol19LBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol19L);
			var vol6LBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol6L);
			var vol1500mlBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol1500ml);
			var vol500mlBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol500ml);

			var traysCount =
				Math.Ceiling(vol19LBottlesCount / _vol19LBottlesOnTrayCount) +
				Math.Ceiling(vol6LBottlesCount / _vol6LBottlesOnTrayCount) +
				Math.Ceiling(vol1500mlBottlesCount / _vol1500mlBottlesOnTrayCount) +
				Math.Ceiling(vol500mlBottlesCount / _vol500mlBottlesOnTrayCount);

			var palletsCount = Math.Ceiling(traysCount / _traysOnPallet);

			var couponsCount = (int)(traysCount + palletsCount);

			return couponsCount;
		}

		private bool IsDocumentHasCommonOrders()
		{
			return _carLoadDocument.Items.Any(x => !x.IsIndividualSetForOrder);
		}

		private decimal GetWaterBottlesCountByTareVolume(TareVolume tareVolume) =>
			_carLoadDocument.Items
			.Where(item =>
				item.Nomenclature.Category == NomenclatureCategory.water
				&& item.Nomenclature.TareVolume.HasValue
				&& item.Nomenclature.TareVolume.Value == tareVolume)
			.Select(item => item.Amount)
			.Sum();

		public static WaterCarLoadDocumentRdl Create(
			UserSettings userSettings,
			CarLoadDocument carLoadDocument,
			Func<int, string, string> qRPlacerFunc)
		{
			var document = new WaterCarLoadDocumentRdl(carLoadDocument, qRPlacerFunc);

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
