using ClosedXML.Excel;
using System.Linq;

namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		public class CashFlowDdsReportRenderer
		{
			private const int _mainCategoryColumn = 1;
			private const int _categoryTitleColumn = 2;
			private const int _categoryMoney = 3;

			public IXLWorkbook Render(CashFlowDdsReport cashFlowDdsReport)
			{
				var result = new XLWorkbook();

				result.AddWorksheet(cashFlowDdsReport.Title);

				var reportSheet = result.Worksheets.First();

				var firstCell = "A1";
				reportSheet.Cell(firstCell).Value = "Отчет по бюджету";
				reportSheet.Cell(firstCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				reportSheet.Cell(firstCell).Style.Font.Bold = true;
				reportSheet.Cell(firstCell).Style.Font.FontSize = 13;
				reportSheet.Range("A1:C1").Row(1).Merge();

				var secondCell = "A2";
				reportSheet.Cell(secondCell).Value = $"с {cashFlowDdsReport.StartDate:dd.MM.yyyy} по {cashFlowDdsReport.EndDate:dd.MM.yyyy}";
				reportSheet.Cell(secondCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				reportSheet.Cell(secondCell).Style.Font.Bold = true;
				reportSheet.Cell(secondCell).Style.Font.FontSize = 13;
				reportSheet.Range("A2:C2").Row(1).Merge();

				reportSheet.Cell("B4").Active = true;

				var summaryRow = reportSheet.ActiveCell.Address.RowNumber;

				reportSheet.Cell(summaryRow, 2).Value = "Прибыль";
				reportSheet.Cell(summaryRow, 2).Style.Font.Bold = true;
				reportSheet.Cell(summaryRow, 2).Style.Font.FontSize = 13;
				reportSheet.Cell(summaryRow, 3).Style.Font.Bold = true;
				reportSheet.Cell(summaryRow, 3).Style.Font.FontSize = 13;

				GoToNextRow(reportSheet);

				GoToNextRow(reportSheet);

				var incomesStartLine = reportSheet.ActiveCell.Address.RowNumber;

				foreach(var incomeGroup in cashFlowDdsReport.IncomesGroupLines)
				{
					RenderIncomeGroup(
						reportSheet,
						incomeGroup,
						1,
						string.Empty);
				}

				GoToNextRow(reportSheet);

				var expensesStartLine = reportSheet.ActiveCell.Address.RowNumber;

				foreach(var expenseGroup in cashFlowDdsReport.ExpensesGroupLines)
				{
					RenderExpenseGroup(
						reportSheet,
						expenseGroup,
						1,
						string.Empty);
				}

				reportSheet.Column(1).AdjustToContents();
				reportSheet.Column(2).AdjustToContents();
				reportSheet.Column(3).AdjustToContents();

				SetMoney(reportSheet, summaryRow, cashFlowDdsReport.IncomesGroupLines.Sum(x => x.Money) - cashFlowDdsReport.ExpensesGroupLines.Sum(x => x.Money), true);

				return result;
			}

			private void RenderIncomeGroup(
				IXLWorksheet xLWorksheet,
				CashFlowDdsReport.IncomesGroupLine incomeGroup,
				int groupLevel,
				string numberPrefix)
			{
				var activeAtStartCellRow = xLWorksheet.ActiveCell.Address.RowNumber;

				SetLeveledGroupTitle(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, groupLevel, incomeGroup.Title, numberPrefix);

				var categoriesStartRowNumber = activeAtStartCellRow + 1;

				GoToNextRow(xLWorksheet);

				var categoriesEndRowNumber = activeAtStartCellRow + incomeGroup.IncomeCategories.Count;

				if(incomeGroup.IncomeCategories.Any())
				{
					foreach(var category in incomeGroup.IncomeCategories)
					{
						RenderIncomeCategory(category, xLWorksheet);
						GoToNextRow(xLWorksheet);
					}
				}

				var incomeGroupsCount = incomeGroup.Groups.Count;

				if(incomeGroup.Groups.Any())
				{
					for(int i = 0; i < incomeGroupsCount; i++)
					{
						RenderIncomeGroup(xLWorksheet, incomeGroup.Groups[i], groupLevel + 1, $"{numberPrefix}{i + 1}. ");
					}
				}

				int groupEndRowNumber = xLWorksheet.ActiveCell.Address.RowNumber - 1;

				if(incomeGroup.IncomeCategories.Any())
				{
					xLWorksheet.Rows(categoriesStartRowNumber, categoriesEndRowNumber).Group();
				}

				if(incomeGroup.Groups.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, groupEndRowNumber).Group();
					}
				}

				SetMoney(xLWorksheet, activeAtStartCellRow, incomeGroup.Money, true);
			}

			private void RenderExpenseGroup(
				IXLWorksheet xLWorksheet,
				CashFlowDdsReport.ExpensesGroupLine expenseGroup,
				int groupLevel,
				string numberPrefix)
			{
				var activeAtStartCellRow = xLWorksheet.ActiveCell.Address.RowNumber;

				SetLeveledGroupTitle(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, groupLevel, expenseGroup.Title, numberPrefix);

				var categoriesStartRowNumber = activeAtStartCellRow + 1;

				GoToNextRow(xLWorksheet);

				var categoriesEndRowNumber = activeAtStartCellRow + expenseGroup.ExpenseCategories.Count();

				if(expenseGroup.ExpenseCategories.Any())
				{
					foreach(var category in expenseGroup.ExpenseCategories)
					{
						RenderExpenseCategory(category, xLWorksheet);
						GoToNextRow(xLWorksheet);
					}
				}

				if(expenseGroup.Groups.Any())
				{
					var expenseGroupsCount = expenseGroup.Groups.Count;
					for(int i = 0; i < expenseGroupsCount; i++)
					{
						RenderExpenseGroup(xLWorksheet, expenseGroup.Groups[i], groupLevel + 1, $"{numberPrefix}{i + 1}. ");
					}
				}

				int groupEndRowNumber = xLWorksheet.ActiveCell.Address.RowNumber - 1;

				if(expenseGroup.ExpenseCategories.Any())
				{
					xLWorksheet.Rows(categoriesStartRowNumber, categoriesEndRowNumber).Group();
				}

				if(expenseGroup.Groups.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, groupEndRowNumber).Group();
					}
				}

				SetMoney(xLWorksheet, activeAtStartCellRow, expenseGroup.Money, true);
			}

			private void RenderIncomeCategory(
				CashFlowDdsReport.FinancialIncomeCategoryLine category,
				IXLWorksheet xLWorksheet)
			{
				SetCategoryRow(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, category.Title, category.Money);
			}

			private void RenderExpenseCategory(
				CashFlowDdsReport.FinancialExpenseCategoryLine category,
				IXLWorksheet xLWorksheet)
			{
				SetCategoryRow(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, category.Title, category.Money);
			}

			private void GoToNextRow(IXLWorksheet xLWorksheet)
			{
				xLWorksheet.Cell(
					xLWorksheet.ActiveCell.Address.RowNumber + 1,
					xLWorksheet.ActiveCell.Address.ColumnNumber).Active = true;
			}

			private void SetLeveledGroupTitle(IXLWorksheet xLWorksheet, int row, int groupLevel, string title, string numberPrefix)
			{
				switch(groupLevel)
				{
					case 1:
						var cell1stLevel = xLWorksheet.Cell(row, _mainCategoryColumn);
						cell1stLevel.Style.Font.Bold = true;
						cell1stLevel.Value = title == "Статьи расхода" ? "Расходы" : "Доходы"; // не очень корректно
						break;
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					default:
						var leveledCell = xLWorksheet.Cell(row, _categoryTitleColumn);
						leveledCell.Style.Font.Bold = true;
						leveledCell.Value = $"{numberPrefix}{title}";
						break;
				}
			}

			private void SetCategoryRow(IXLWorksheet xLWorksheet, int row, string title, decimal money)
			{
				var titleCell = xLWorksheet.Cell(row, _categoryTitleColumn);

				titleCell.Value = title;

				SetMoney(xLWorksheet, row, money);
			}

			private void SetMoney(IXLWorksheet xLWorksheet, int row, decimal money, bool bold = false)
			{
				var moneyCell = xLWorksheet.Cell(row, _categoryMoney);

				if(bold)
				{
					moneyCell.Style.Font.Bold = true;
				}

				moneyCell.Style.NumberFormat.Format = "# ##0.00 ₽;-# ##0.00 ₽";
				moneyCell.DataType = XLDataType.Number;
				moneyCell.Value = money;
			}
		}
	}
}
