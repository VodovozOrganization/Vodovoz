using ClosedXML.Excel;
using DateTimeHelpers;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private const string _complaintsSummaryWorksheetName = "Сводка рекламаций";
		private const string _complaintsListWorksheetName = "Рекламации поступившие сегодня";
		private const string _undeliveredOrdersWorksheetName = "Недовозы";
		private const string _discountsWorksheetName = "Отчет по скидкам";
		private const string _dateFormatString = "dd.MM.yyyy";

		private DateTime _date;

		private OksDailyReport() { }

		public void ExportReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var complaintsSummaryWorksheet = workbook.Worksheets.Add(_complaintsSummaryWorksheetName);
				FillComplaintsSummaryWorksheet(ref complaintsSummaryWorksheet);

				var complaintsListWorksheet = workbook.Worksheets.Add(_complaintsListWorksheetName);
				FillComplaintsListWorksheet(ref complaintsListWorksheet);

				var undeliveredOrdersWorksheet = workbook.Worksheets.Add(_undeliveredOrdersWorksheetName);
				FillUndeliveredOrdersWorksheet(ref undeliveredOrdersWorksheet);

				var discountsWorksheet = workbook.Worksheets.Add(_discountsWorksheetName);
				FillDiscountsWorksheet(ref discountsWorksheet);

				workbook.SaveAs(path);
			}
		}

		private void FormatCells(
			IXLRange cellsRange,
			double fontSize = 11,
			bool isBoldFont = false,
			bool isWrapText = true,
			XLColor bgColor = null,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Left,
			XLAlignmentVerticalValues verticalAlignment = XLAlignmentVerticalValues.Center,
			XLBorderStyleValues cellBorderStyle = XLBorderStyleValues.None)
		{
			cellsRange.Cells().Style.Font.FontSize = fontSize;
			cellsRange.Cells().Style.Font.Bold = isBoldFont;

			if(bgColor != null)
			{
				FillCellBackground(cellsRange, bgColor);
			}

			cellsRange.Cells().Style.Alignment.WrapText = isWrapText;
			cellsRange.Cells().Style.Alignment.Horizontal = horizontalAlignment;
			cellsRange.Cells().Style.Alignment.Vertical = verticalAlignment;
			cellsRange.Cells().Style.Border.OutsideBorder = cellBorderStyle;
		}

		private void FillCellBackground(IXLRange cellsRange, XLColor color)
		{
			cellsRange.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = color;
			cellsRange.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = color;
		}

		private void Initialize(
			IUnitOfWork uow,
			DateTime date,
			IComplaintsRepository complaintsRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IOrderRepository orderRepository,
			IComplaintSettings complaintSettings,
			ISubdivisionSettings subdivisionSettings,
			IOrderSettings orderSettings)
		{
			_date = date;
			_incomingCallSourseId = complaintSettings.IncomeCallComplaintSourceId;
			_oksSubdivisionId = subdivisionSettings.GetOkkId();

			_oksDiscountReasonsIds = orderSettings.OksDiscountReasonsIds;
			_productChangeDiscountReasonsIds = orderSettings.ProductChangeDiscountReasonsIds;
			_additionalDeliveryDiscountReasonsIds = orderSettings.AdditionalDeliveryDiscountReasonsIds;

			_complaintsDataOnDate =
				complaintsRepository
				.GetClientComplaintsForPeriod(uow, date, date)
				.ToList();

			_complaintsDataFromMonthBeginningToDate =
				complaintsRepository
				.GetClientComplaintsForPeriod(uow, date.FirstDayOfMonth(), date)
				.ToList();

			_undeliveredOrdersDataForDate =
				undeliveredOrdersRepository
				.GetUndeliveredOrdersForPeriod(uow, date, date)
				.ToList();

			_undeliveredOrdersDataFromMonthBeginningToDate =
				undeliveredOrdersRepository
				.GetUndeliveredOrdersForPeriod(uow, date.FirstDayOfMonth(), date)
				.ToList();

			_ordersDiscountsDataForDate =
				orderRepository
				.GetOrdersDiscountsDataForPeriod(uow, date, date)
				.ToList();
		}

		public static OksDailyReport Create(
			IUnitOfWork uow,
			DateTime date,
			IComplaintsRepository complaintsRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IOrderRepository orderRepository,
			IComplaintSettings complaintSettings,
			ISubdivisionSettings subdivisionSettings,
			IOrderSettings orderSettings)
		{
			var report = new OksDailyReport();

			report.Initialize(
				uow,
				date,
				complaintsRepository,
				undeliveredOrdersRepository,
				orderRepository,
				complaintSettings,
				subdivisionSettings,
				orderSettings);

			return report;
		}
	}
}
