using ClosedXML.Excel;
using System.Drawing;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _undeliveredOrdersTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(222, 235, 247));
		private readonly XLColor _undeliveredOrdersMarkupBgColor = XLColor.FromColor(Color.FromArgb(146, 208, 80));

		private void FillUndeliveredOrdersWorksheet(ref IXLWorksheet worksheet)
		{
			SetUndeliveredOrdersColumnsWidth(ref worksheet);
			AddUndeliveredOrdersTitleTable(ref worksheet);
		}

		private void SetUndeliveredOrdersColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 4;
			worksheet.Column(2).Width = 10;
			worksheet.Column(3).Width = 18;
			worksheet.Column(4).Width = 24;
			worksheet.Column(5).Width = 24;
			worksheet.Column(6).Width = 24;
			worksheet.Column(7).Width = 20;
			worksheet.Column(8).Width = 48;
			worksheet.Column(9).Width = 20;
			worksheet.Column(10).Width = 48;
		}

		private void AddUndeliveredOrdersTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 2;

			worksheet.Cell(rowNumber, 2).Value = $"Отчет по недовозам {Date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, 2, rowNumber, 10);
			cellsRange.Merge();

			FormatUndeliveredOrdersWorksheetsTitleCells(cellsRange);
		}

		private void FormatUndeliveredOrdersWorksheetsTitleCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 16,
				isBoldFont: true,
				bgColor: _undeliveredOrdersTitlesMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}
	}
}
