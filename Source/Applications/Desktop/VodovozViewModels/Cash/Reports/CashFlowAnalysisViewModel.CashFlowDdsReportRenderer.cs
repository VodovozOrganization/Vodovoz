using ClosedXML.Excel;
using System;
using System.Drawing;
using System.Linq;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public class CashFlowDdsReportRenderer
		{
			private XLColor _subTotalsBGXLColor;

			private const int _splitterRowsCount = 0;

			private const string _worksheetTitle = "Отчет";

			private const string _headerTitle = "Анализ движения денежных средств";
			private const int _headerRow = 1;
			private const int _headerFontSize = 13;

			private const int _subHeaderRow = 2;
			private const int _subHeaderFontSize = 13;

			private const int _groupTitleFontSize = 11;

			private const int _mainCategoryColumn = 1;
			private const int _categoryTitleColumn = 2;
			private const int _categoryFontSize = 11;
			private const int _moneyColumn = 3;

			private const string _moneyNumericFormat = "# ### ### ##0.00;-# ### ### ##0.00;-";

			private const string _incomesHeaderTitle = "Доходы";
			private const int _incomesHeaderFontSize = 13;

			private const string _expensesHeaderTitle = "Расходы";
			private const int _expensesHeaderFontSize = 13;

			private const string _totalHeaderTitle = "Прибыль";
			private const int _totalHeaderFontSize = 13;

			public IXLWorkbook Render(CashFlowDdsReport cashFlowDdsReport, Color accentColot)
			{
				_subTotalsBGXLColor = XLColor.FromColor(accentColot);

				var result = new XLWorkbook();

				result.AddWorksheet(_worksheetTitle);

				var reportSheet = result.Worksheets.First();

				RenderHeader(reportSheet);

				RenderSubheader(reportSheet, cashFlowDdsReport.StartDate, cashFlowDdsReport.EndDate);

				var incomesStartLine = 4;

				var incomesSum = cashFlowDdsReport.IncomesGroupLines.Sum(x => x.Money);

				var insertedIncomesGroupRows = RenderIncomesLines(reportSheet, cashFlowDdsReport, incomesStartLine, incomesSum);

				var expensesSum = cashFlowDdsReport.ExpensesGroupLines.Sum(x => x.Money);

				int insertedExpensesLines = RenderExpensesLines(reportSheet, cashFlowDdsReport, incomesStartLine + insertedIncomesGroupRows, expensesSum);

				RenderSubSubheader(
					reportSheet,
					incomesStartLine + insertedIncomesGroupRows + insertedExpensesLines,
					_totalHeaderTitle,
					_totalHeaderFontSize,
					incomesSum - expensesSum);

				reportSheet.Column(_mainCategoryColumn).AdjustToContents();
				reportSheet.Column(_categoryTitleColumn).AdjustToContents();
				reportSheet.Column(_moneyColumn).AdjustToContents();

				reportSheet.CollapseRows();

				return result;
			}

			private int RenderIncomesLines(IXLWorksheet reportSheet, CashFlowDdsReport cashFlowDdsReport, int row, decimal incomesSum)
			{
				RenderSubSubheader(
					reportSheet,
					row,
					_incomesHeaderTitle,
					_incomesHeaderFontSize,
					incomesSum);

				var incomegroupsStartAt = row;

				var insertedIncomesGroupRows = 0;

				foreach(var incomeGroup in cashFlowDdsReport.IncomesGroupLines)
				{
					insertedIncomesGroupRows += RenderIncomeGroup(
						reportSheet,
						incomegroupsStartAt + insertedIncomesGroupRows,
						incomeGroup,
						1,
						string.Empty);
				}

				return insertedIncomesGroupRows;
			}

			private int RenderExpensesLines(IXLWorksheet reportSheet, CashFlowDdsReport cashFlowDdsReport, int row, decimal sum)
			{
				RenderSubSubheader(
					reportSheet,
					row,
					_expensesHeaderTitle,
					_expensesHeaderFontSize,
					sum);

				var expensesStartAt = row;

				var insertedExpensesGroupRows = 0;

				foreach(var expenseGroup in cashFlowDdsReport.ExpensesGroupLines)
				{
					insertedExpensesGroupRows += RenderExpenseGroup(
						reportSheet,
						expensesStartAt + insertedExpensesGroupRows,
						expenseGroup,
						1,
						string.Empty);
				}

				return insertedExpensesGroupRows;
			}

			private void RenderSubSubheader(IXLWorksheet xLWorksheet, int row, string title, int fontSize, decimal money, bool bold = true)
			{
				var subheaderCell = xLWorksheet.Cell(row, _mainCategoryColumn);

				ApplyBGColorConditionalFormat(
					xLWorksheet.Range(row, _mainCategoryColumn, row, _moneyColumn),
					_subTotalsBGXLColor);

				subheaderCell.Style.Font.Bold = bold;
				subheaderCell.Style.Font.FontSize = fontSize;
				subheaderCell.Value = title;

				RenderMoney(xLWorksheet,
					row,
					money,
					fontSize,
					bold);
			}

			private void RenderHeader(IXLWorksheet xLWorksheet)
			{
				var firstCell = xLWorksheet.Cell(_headerRow, 1);
				firstCell.Value = _headerTitle;
				firstCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				firstCell.Style.Font.Bold = true;
				firstCell.Style.Font.FontSize = _headerFontSize;
				xLWorksheet.Range(_headerRow, 1, _headerRow, 3).Merge();
			}

			private void RenderSubheader(IXLWorksheet reportSheet, DateTime startDate, DateTime endDate)
			{
				var subheaderFirstCell = reportSheet.Cell(_subHeaderRow, 1);
				subheaderFirstCell.Value = $"с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
				subheaderFirstCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				subheaderFirstCell.Style.Font.Bold = true;
				subheaderFirstCell.Style.Font.FontSize = _subHeaderFontSize;
				reportSheet.Range(_subHeaderRow, 1, _subHeaderRow, 3).Merge();
			}

			private void ApplyBGColorConditionalFormat(IXLRange range, XLColor color)
			{
				range.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = color;
				range.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = color;
			}

			private int RenderIncomeGroup(
				IXLWorksheet xLWorksheet,
				int startRow,
				CashFlowDdsReport.IncomesGroupLine incomeGroup,
				int groupLevel,
				string numberPrefix)
			{
				var inserted = 0;

				if(groupLevel > 1)
				{
					RenderGroupTitle(xLWorksheet, startRow, incomeGroup.Title, numberPrefix, incomeGroup.Money);
				}

				inserted++;

				var incomeGroupsCount = incomeGroup.Groups.Count;

				if(incomeGroupsCount != 0)
				{
					for(int i = 0; i < incomeGroupsCount; i++)
					{
						inserted += RenderIncomeGroup(xLWorksheet, startRow + inserted, incomeGroup.Groups[i], groupLevel + 1, $"{numberPrefix}{i + 1}. ");
					}
				}

				int groupEndRowNumber = startRow + inserted;

				var categoriesEndRowNumber = incomeGroup.IncomeCategories.Count + startRow + inserted;

				if(incomeGroup.IncomeCategories.Count != 0)
				{
					foreach(var category in incomeGroup.IncomeCategories)
					{
						RenderIncomeCategory(xLWorksheet, startRow + inserted, category);
						inserted++;
					}
				}

				var categoriesStartRowNumber = groupEndRowNumber + 1;

				if(incomeGroup.Groups.Count != 0)
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(startRow + 1, categoriesEndRowNumber - 1).Group();
					}
				}

				if(incomeGroup.IncomeCategories.Count != 0)
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber - 1, categoriesEndRowNumber - 1).Group();
					}
				}

				return inserted;
			}

			private int RenderExpenseGroup(
				IXLWorksheet xLWorksheet,
				int startRow,
				CashFlowDdsReport.ExpensesGroupLine expenseGroup,
				int groupLevel,
				string numberPrefix)
			{
				var inserted = 0;

				if(groupLevel > 1)
				{
					RenderGroupTitle(xLWorksheet, startRow, expenseGroup.Title, numberPrefix, expenseGroup.Money);
				}

				inserted++;

				var expenseGroupsCount = expenseGroup.Groups.Count;

				if(expenseGroupsCount != 0)
				{
					for(int i = 0; i < expenseGroupsCount; i++)
					{
						inserted += RenderExpenseGroup(xLWorksheet, startRow + inserted, expenseGroup.Groups[i], groupLevel + 1, $"{numberPrefix}{i + 1}. ");
					}
				}

				int groupEndRowNumber = startRow + inserted;

				var categoriesEndRowNumber = expenseGroup.ExpenseCategories.Count + startRow + inserted;

				if(expenseGroup.ExpenseCategories.Count != 0)
				{
					foreach(var category in expenseGroup.ExpenseCategories)
					{
						RenderExpenseCategory(xLWorksheet, startRow + inserted, category);
						inserted++;
					}
				}

				var categoriesStartRowNumber = groupEndRowNumber + 1;

				if(expenseGroup.Groups.Count != 0)
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(startRow + 1, categoriesEndRowNumber - 1).Group();
					}
				}

				if(expenseGroup.ExpenseCategories.Count != 0)
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber - 1, categoriesEndRowNumber - 1).Group();
					}
				}

				return inserted;
			}

			private void RenderIncomeCategory(
				IXLWorksheet xLWorksheet,
				int row,
				CashFlowDdsReport.FinancialIncomeCategoryLine category)
			{
				RenderCategoryRow(xLWorksheet, row, category.Title, category.Money);
			}

			private void RenderExpenseCategory(
				IXLWorksheet xLWorksheet,
				int row,
				CashFlowDdsReport.FinancialExpenseCategoryLine category)
			{
				RenderCategoryRow(xLWorksheet, row, category.Title, category.Money);
			}

			private void RenderGroupTitle(IXLWorksheet xLWorksheet, int row, string title, string numberPrefix, decimal money)
			{
				var groupTitleCell = xLWorksheet.Cell(row, _categoryTitleColumn);
				groupTitleCell.Style.Font.Bold = true;
				groupTitleCell.Style.Font.FontSize = _groupTitleFontSize;
				groupTitleCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

				groupTitleCell.Value = $"{numberPrefix}{title}";

				RenderMoney(xLWorksheet, row, money, _groupTitleFontSize, true, true);
			}

			private void RenderCategoryRow(IXLWorksheet xLWorksheet, int row, string title, decimal money, bool right = false)
			{
				var titleCell = xLWorksheet.Cell(row, _categoryTitleColumn);

				titleCell.Value = title;
				titleCell.Style.Font.FontSize = _categoryFontSize;
				titleCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

				if(right)
				{
					titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				}

				RenderMoney(xLWorksheet, row, money, _categoryFontSize, border: true);
			}

			private void RenderMoney(IXLWorksheet xLWorksheet, int row, decimal money, int fontSize, bool bold = false, bool border = false)
			{
				var moneyCell = xLWorksheet.Cell(row, _moneyColumn);

				if(bold)
				{
					moneyCell.Style.Font.Bold = true;
				}

				if(border)
				{
					moneyCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				}

				moneyCell.Style.NumberFormat.Format = _moneyNumericFormat;
				moneyCell.DataType = XLDataType.Number;
				moneyCell.Style.Font.FontSize = fontSize;
				moneyCell.Value = money;
			}
		}
	}
}
