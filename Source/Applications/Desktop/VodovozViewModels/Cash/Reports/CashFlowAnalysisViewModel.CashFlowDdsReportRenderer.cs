using ClosedXML.Excel;
using System;
using System.Drawing;
using Vodovoz.Models.Cash;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		/// <summary>
		/// Выгружает сформированный отчёт ДДС или ДДР в Excel
		/// </summary>
		public class CashFlowDdsReportRenderer
		{
			private const string _worksheetTitle = "Отчет";
			private const string _headerTitle = "Анализ движения денежных средств";
			private const string _moneyNumericFormat = "# ### ### ##0.00;-# ### ### ##0.00;-";

			private const int _headerRow = 1;
			private const int _subHeaderRow = 2;
			private const int _firstSectionRow = 4;
			private const int _numberingColumn = 1;
			private const int _titleColumn = 2;
			private const int _moneyColumn = 3;
			private const int _headerFontSize = 13;
			private const int _detailFontSize = 11;
			private const int _maxOutlineLevel = 7;

			/// <summary>
			/// Записывает строки последнего сформированного отчёта
			/// </summary>
			/// <param name="report">Дерево и итоги сформированного отчёта</param>
			/// <param name="accentColor">Цвет разделов и прибыли</param>
			/// <returns>Книга Excel</returns>
			public IXLWorkbook Render(CashFlowDdsReport report, Color accentColor)
			{
				var workbook = new XLWorkbook();
				var sheet = workbook.AddWorksheet(_worksheetTitle);
				sheet.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Top;

				RenderHeader(sheet, _headerRow, _headerTitle);
				RenderHeader(sheet, _subHeaderRow, $"с {report.StartDate:dd.MM.yyyy} по {report.EndDate:dd.MM.yyyy}");

				var totalsColor = XLColor.FromColor(accentColor);
				var row = RenderSection(sheet, _firstSectionRow, report.IncomeSection, totalsColor);
				row = RenderSection(sheet, row, report.ExpenseSection, totalsColor);
				RenderSection(sheet, row, report.ProfitLine, totalsColor);

				sheet.Column(_numberingColumn).AdjustToContents();
				sheet.Column(_titleColumn).AdjustToContents();
				sheet.Column(_moneyColumn).AdjustToContents();
				sheet.CollapseRows();

				return workbook;
			}

			private static int RenderSection(IXLWorksheet sheet, int row, CashFlowReportLine section, XLColor color)
			{
				var titleCell = sheet.Cell(row, _numberingColumn);
				titleCell.Value = section.Title;
				titleCell.Style.Font.Bold = true;
				titleCell.Style.Font.FontSize = _headerFontSize;

				var sectionRange = sheet.Range(row, _numberingColumn, row, _moneyColumn);
				sectionRange.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = color;
				sectionRange.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = color;
				RenderMoney(sheet, row, section.Money, _headerFontSize, bold: true, border: false);

				var nextRow = row + 1;

				foreach(var child in section.Children)
				{
					nextRow = RenderDetail(sheet, nextRow, child, 1);
				}

				return nextRow;
			}

			private static int RenderDetail(IXLWorksheet sheet, int row, CashFlowReportLine line, int depth)
			{
				var bold = !line.IsArticle || line.Children.Count > 0;
				var numberingCell = sheet.Cell(row, _numberingColumn);

				if(!string.IsNullOrEmpty(line.Numbering))
				{
					numberingCell.Value = $"'{line.Numbering}";
				}

				numberingCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				StyleDetailCell(numberingCell, bold);

				var titleCell = sheet.Cell(row, _titleColumn);
				titleCell.Value = line.Title;
				titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				titleCell.Style.Alignment.Indent = depth;
				StyleDetailCell(titleCell, bold);

				RenderMoney(sheet, row, line.Money, _detailFontSize, bold, border: true);
				sheet.Row(row).OutlineLevel = Math.Min(depth - 1, _maxOutlineLevel);

				var nextRow = row + 1;

				foreach(var child in line.Children)
				{
					nextRow = RenderDetail(sheet, nextRow, child, depth + 1);
				}

				return nextRow;
			}

			private static void RenderHeader(IXLWorksheet sheet, int row, string title)
			{
				var cell = sheet.Cell(row, _numberingColumn);
				cell.Value = title;
				cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				cell.Style.Font.Bold = true;
				cell.Style.Font.FontSize = _headerFontSize;
				sheet.Range(row, _numberingColumn, row, _moneyColumn).Merge();
			}

			private static void StyleDetailCell(IXLCell cell, bool bold)
			{
				cell.Style.Font.Bold = bold;
				cell.Style.Font.FontSize = _detailFontSize;
				cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			}

			private static void RenderMoney(IXLWorksheet sheet, int row, decimal money, int fontSize, bool bold, bool border)
			{
				var cell = sheet.Cell(row, _moneyColumn);
				cell.Style.Font.Bold = bold;
				cell.Style.Font.FontSize = fontSize;

				if(border)
				{
					cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				}

				cell.Style.NumberFormat.Format = _moneyNumericFormat;
				cell.DataType = XLDataType.Number;
				cell.Value = money;
			}
		}
	}
}
