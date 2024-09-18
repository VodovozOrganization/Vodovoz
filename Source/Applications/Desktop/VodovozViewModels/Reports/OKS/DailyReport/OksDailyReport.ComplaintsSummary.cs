using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
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
			var excelRowCounter = 1;

			AddTableTitleRow(worksheet, excelRowCounter);
			excelRowCounter++;
		}

		private void AddTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			worksheet.Cell(rowNumber, 1).Value = $"Отчет по рекламациям ОКС {Date.ToString(_dateFormatString)}";

			var tableTitleRange = worksheet.Range(rowNumber, 1, rowNumber, 15);
			tableTitleRange.Merge();
			FormatTitleCells(tableTitleRange);
		}

		private void FormatTitleCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Font.Bold = true;
			cellsRange.Cells().Style.Font.FontSize = 22;

			FillCellBackground(cellsRange, _complaintsSummaryMarkupBgColor);

			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void FillCellBackground(IXLRange cellsRange, XLColor color)
		{
			cellsRange.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = color;
			cellsRange.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = color;
		}
	}
}
