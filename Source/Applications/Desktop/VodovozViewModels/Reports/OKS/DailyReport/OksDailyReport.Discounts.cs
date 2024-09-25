using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _discountsTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(220, 197, 225));

		public IList<OksDailyReportOrderDiscountDataNode> OrdersDiscountsDataForDate { get; private set; } =
			new List<OksDailyReportOrderDiscountDataNode>();

		public IList<OksDailyReportOrderDiscountDataNode> OrdersDiscountsDataFromMonthBeginningToDate { get; private set; } =
			new List<OksDailyReportOrderDiscountDataNode>();

		private void FillDiscountsWorksheet(ref IXLWorksheet worksheet)
		{
			SetDiscountsColumnsWidth(ref worksheet);
			AddDiscountsTitleTable(ref worksheet);
		}

		private void SetDiscountsColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 4;
			worksheet.Column(2).Width = 10;
			worksheet.Column(3).Width = 44;
			worksheet.Column(4).Width = 10;
			worksheet.Column(5).Width = 16;
			worksheet.Column(6).Width = 16;
			worksheet.Column(7).Width = 16;
			worksheet.Column(8).Width = 16;
			worksheet.Column(9).Width = 48;
			worksheet.Column(10).Width = 16;
		}

		private void AddDiscountsTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 2;

			worksheet.Cell(rowNumber, 2).Value = $"Отчет по скидкам {Date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, 2, rowNumber, 10);
			cellsRange.Merge();

			FormatDiscountsWorksheetsTitleCells(cellsRange);
		}

		private void FormatDiscountsWorksheetsTitleCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 16,
				isBoldFont: true,
				bgColor: _discountsTitlesMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}
	}
}
