using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.PrintableDocuments;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Documents;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.Print.Store
{
	public class WaterCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocument.rdl";

		private const int _vol19LBottlesOnPalletCount = 40;
		private const int _vol19LOneNomenclatureBottlesOnPalletCount = 48;
		private const int _vol6LBottlesOnTrayCount = 10;
		private const int _vol1500mlBottlesOnTrayCount = 30;
		private const int _vol500mlBottlesOnTrayCount = 60;
		private const int _traysOnPallet = 3;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly CarLoadDocument _carLoadDocument;
		private readonly Func<int, string, string> _qRPlacerFunc;
		private int _copiesToPrint;
		private string _printerName;

		private WaterCarLoadDocumentRdl(
			IReportInfoFactory reportInfoFactory,
			CarLoadDocument carLoadDocument,
			Func<int, string, string> qRPlacerFunc)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_carLoadDocument = carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
			_qRPlacerFunc = qRPlacerFunc ?? throw new ArgumentNullException(nameof(qRPlacerFunc));
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
			var source = _qRPlacerFunc.Invoke(_carLoadDocument.Id, GetReportSource());

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Source = source;
			reportInfo.Title = Name;
			reportInfo.Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } };
			reportInfo.PrintType = ReportInfo.PrintingType.MultiplePrinters;
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

			modifier.Setup(_carLoadDocument.SeparateTableOrderIds, tearOffCouponsCount, _carLoadDocument.IsDocumentHasCommonOrders);

			return modifier;
		}

		private int GetTearOffCouponsCount()
		{
			var groupedByOrdersItems = _carLoadDocument.Items
				.GroupBy(item => (item.OrderId, item.IsIndividualSetForOrder))
				.ToDictionary(g => g.Key, g => g.ToList());

			var couponsCount = 0;

			foreach(var item in groupedByOrdersItems)
			{
				couponsCount += GetPalletsCountFor19LBottles(item.Value);
				couponsCount += GetPalletsCountForSmallBottles(item.Value);
			}

			return couponsCount;
		}

		private int GetPalletsCountForSmallBottles(IEnumerable<CarLoadDocumentItem> items)
		{
			var vol6LBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol6L, items);
			var vol1500mlBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol1500ml, items);
			var vol500mlBottlesCount = GetWaterBottlesCountByTareVolume(TareVolume.Vol500ml, items);

			var smallBottlesTraysCount =
				Math.Ceiling(vol6LBottlesCount / _vol6LBottlesOnTrayCount) +
				Math.Ceiling(vol1500mlBottlesCount / _vol1500mlBottlesOnTrayCount) +
				Math.Ceiling(vol500mlBottlesCount / _vol500mlBottlesOnTrayCount);

			var palletsCount = (int)Math.Ceiling(smallBottlesTraysCount / _traysOnPallet);

			return palletsCount;
		}

		private int GetPalletsCountFor19LBottles(IEnumerable<CarLoadDocumentItem> items)
		{
			var groupedItems = items
				.Where(item =>
					item.Nomenclature.Category == NomenclatureCategory.water
					&& item.Nomenclature.TareVolume.HasValue
					&& item.Nomenclature.TareVolume.Value == TareVolume.Vol19L)
				.GroupBy(item => item.Nomenclature.Id)
				.ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

			int palletsCount = default;
			decimal bottlesOnCompositePallet = default;

			foreach(var item in groupedItems)
			{
				var fuelPalletsCount = (int)(item.Value / _vol19LOneNomenclatureBottlesOnPalletCount);
				bottlesOnCompositePallet += item.Value - fuelPalletsCount * _vol19LOneNomenclatureBottlesOnPalletCount;

				palletsCount += fuelPalletsCount;
			}

			palletsCount += (int)Math.Ceiling(bottlesOnCompositePallet / _vol19LBottlesOnPalletCount);

			return palletsCount;
		}

		private decimal GetWaterBottlesCountByTareVolume(TareVolume tareVolume, IEnumerable<CarLoadDocumentItem> items) =>
			items
			.Where(item =>
				item.Nomenclature.Category == NomenclatureCategory.water
				&& item.Nomenclature.TareVolume.HasValue
				&& item.Nomenclature.TareVolume.Value == tareVolume)
			.Select(item => item.Amount)
			.Sum();

		public static WaterCarLoadDocumentRdl Create(
			UserSettings userSettings,
			CarLoadDocument carLoadDocument,
			Func<int, string, string> qRPlacerFunc,
			IReportInfoFactory reportInfoFactory
			)
		{
			var document = new WaterCarLoadDocumentRdl(reportInfoFactory, carLoadDocument, qRPlacerFunc);

			var savedPrinterSettings = userSettings.GetPrinterSettingByDocumentType(document.DocumentType);

			document.CopiesToPrint = savedPrinterSettings is null ? 1 : savedPrinterSettings.NumberOfCopies;
			document.PrinterName = savedPrinterSettings?.PrinterName;

			return document;
		}
	}
}
