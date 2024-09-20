using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _complaintsSummaryMarkupBgColor = XLColor.FromColor(Color.FromArgb(226, 240, 217));

		public IList<OksDailyReportComplaintDataNode> ComplaintsDataForDate { get; private set; } =
			new List<OksDailyReportComplaintDataNode>();
		public IList<OksDailyReportComplaintDataNode> ComplaintsDataFromMonthBeginningToDate { get; private set; } =
			new List<OksDailyReportComplaintDataNode>();

		private void FillComplaintsSummaryWorksheet(ref IXLWorksheet worksheet)
		{
			SetComplaintsSummaryColumnsWidth(ref worksheet);
			AddComplaintsSummaryTitleTable(ref worksheet);
			AddComplaintsSummaryByComplaintSourceTable(ref worksheet);
		}

		private void SetComplaintsSummaryColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 8;
			worksheet.Column(2).Width = 24;
			worksheet.Column(3).Width = 12;
			worksheet.Column(4).Width = 8;
			worksheet.Column(5).Width = 24;
			worksheet.Column(6).Width = 12;
			worksheet.Column(7).Width = 8;
			worksheet.Column(8).Width = 12;
			worksheet.Column(9).Width = 12;
			worksheet.Column(10).Width = 12;
			worksheet.Column(11).Width = 8;
			worksheet.Column(12).Width = 12;
			worksheet.Column(13).Width = 12;
			worksheet.Column(14).Width = 12;
		}

		private void AddComplaintsSummaryTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 1;

			worksheet.Cell(rowNumber, 2).Value = $"Отчет по рекламациям ОКС {Date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, 2, rowNumber, 16);
			cellsRange.Merge();

			FormatComplaintsSummaryWorksheetTitleCells(cellsRange);
		}

		private void AddComplaintsSummaryByComplaintSourceTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 3;
			var labelColumnNumber = 2;
			var dataColumnNumber = labelColumnNumber + 1;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Всего рекламаций за смену:";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Count;

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Входящие звонки";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Where(c => c.ComplaintSource.Id == IncomingCallSourseId).Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Чат \"Обращения\"";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Where(c => c.ComplaintSource.Id != IncomingCallSourseId).Count();

			FormatComplaintsSummaryLabelCells(worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber));
			FormatComplaintsSummaryDataCells(worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));
		}

		private void FormatComplaintsSummaryWorksheetTitleCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 22,
				isBoldFont: true,
				bgColor: _complaintsSummaryMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryLabelCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _complaintsSummaryMarkupBgColor,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryDataCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 12,
				isBoldFont: true,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}
	}
}
