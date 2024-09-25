using ClosedXML.Excel;
using DateTimeHelpers;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Settings.Complaints;
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

		private OksDailyReport() { }

		public DateTime Date { get; private set; }
		public int IncomingCallSourseId { get; private set; }

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

		public static OksDailyReport Create(
			IUnitOfWork uow,
			DateTime date,
			IComplaintsRepository complaintsRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IOrderRepository orderRepository,
			IComplaintSettings complaintSettings,
			ISubdivisionSettings subdivisionSettings)
		{
			var report = new OksDailyReport();

			report.Date = date;
			report.IncomingCallSourseId = complaintSettings.IncomeCallComplaintSourceId;

			report.ComplaintsDataForDate =
				complaintsRepository
				.GetClientComplaintsForPeriod(uow, date, date, subdivisionSettings.GetOkkId())
				.ToList();

			report.ComplaintsDataFromMonthBeginningToDate =
				complaintsRepository
				.GetClientComplaintsForPeriod(uow, date.FirstDayOfMonth(), date, subdivisionSettings.GetOkkId())
				.ToList();

			report.UndeliveredOrdersDataForDate =
				undeliveredOrdersRepository
				.GetUndeliveredOrdersForPeriod(uow, date, date)
				.ToList();

			report.UndeliveredOrdersDataFromMonthBeginningToDate =
				undeliveredOrdersRepository
				.GetUndeliveredOrdersForPeriod(uow, date.FirstDayOfMonth(), date)
				.ToList();

			report.OrdersDiscountsDataForDate =
				orderRepository
				.GetOrdersDiscountsDataForPeriod(uow, date, date)
				.ToList();

			report.OrdersDiscountsDataFromMonthBeginningToDate =
				orderRepository
				.GetOrdersDiscountsDataForPeriod(uow, date.FirstDayOfMonth(), date)
				.ToList();

			return report;
		}
	}
}
