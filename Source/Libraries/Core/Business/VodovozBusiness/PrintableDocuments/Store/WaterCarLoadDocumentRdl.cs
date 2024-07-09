using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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


			//var reportInfo = new ReportInfo
			//{
			//	Source = _source,
			//	Parameters = Parameters,
			//	Title = Name,
			//	UseUserVariables = true
			//};

			//return reportInfo;

			return _reportInfo;
		}

		//private string GetReportSource()
		//{
		//	var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		//	var fileName = IsDetailed ? "ProfitabilitySalesReportDetail.rdl" : "ProfitabilitySalesReport.rdl";
		//	var path = Path.Combine(root, "Reports", "Sales", fileName);

		//	return ModifyReport(path);
		//}

		//private string ModifyReport(string path)
		//{
		//	var modifier = GetReportModifier();

		//	using(ReportController reportController = new ReportController(path))
		//	using(var reportStream = new MemoryStream())
		//	{
		//		reportController.AddModifier(modifier);
		//		reportController.Modify();
		//		reportController.Save(reportStream);

		//		using(var reader = new StreamReader(reportStream))
		//		{
		//			reportStream.Position = 0;
		//			var outputSource = reader.ReadToEnd();
		//			return outputSource;
		//		}
		//	}
		//}

		//private ReportModifierBase GetReportModifier()
		//{
		//	ReportModifierBase result;
		//	var groupParameters = GetGroupingParameters();
		//	if(IsDetailed)
		//	{
		//		var modifier = new ProfitabilityDetailReportModifier();
		//		modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value));
		//		result = modifier;

		//	}
		//	else
		//	{
		//		var isRouteListGroupingTypeSelected = groupParameters.Select(x => (GroupingType)x.Value).First() == GroupingType.RouteList;
		//		var isOnlyOneGroupingTypeSelected = groupParameters.Count() == 1;
		//		var isShowRouteListInfo = isRouteListGroupingTypeSelected && isOnlyOneGroupingTypeSelected;

		//		var modifier = new ProfitabilityReportModifier();
		//		modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value), isShowRouteListInfo);
		//		result = modifier;
		//	}
		//	return result;
		//}

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
