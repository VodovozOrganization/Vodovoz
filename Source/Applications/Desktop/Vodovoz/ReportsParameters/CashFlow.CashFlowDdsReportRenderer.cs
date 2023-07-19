using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		public class CashFlowDdsReportRenderer
		{
			private readonly XLColor _subTotalsBGColor = XLColor.FromColor(Color.FromArgb(253, 233, 216));

			private const int _mainCategoryColumn = 1;
			private const int _categoryTitleColumn = 2;
			private const int _categoryMoney = 3;

			// Исключен тип Возврат от подотчетного лица
			private readonly int _incomeTypesCount = Enum.GetValues(typeof(IncomeType)).Length - 1;

			// Включен тип приходного ордера Возврат от подотчетного лица
			private readonly int _expenseTypesCount = Enum.GetValues(typeof(ExpenseType)).Length + 1;

			public IXLWorkbook Render(CashFlowDdsReport cashFlowDdsReport)
			{
				var result = new XLWorkbook();

				result.AddWorksheet(cashFlowDdsReport.Title);

				var reportSheet = result.Worksheets.First();

				var firstCell = reportSheet.Cell("A1");
				firstCell.Value = "Отчет по бюджету";
				firstCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				firstCell.Style.Font.Bold = true;
				firstCell.Style.Font.FontSize = 13;
				reportSheet.Range("A1:C1").Row(1).Merge();

				var secondCell = reportSheet.Cell("A2");
				secondCell.Value = $"с {cashFlowDdsReport.StartDate:dd.MM.yyyy} по {cashFlowDdsReport.EndDate:dd.MM.yyyy}";
				secondCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				secondCell.Style.Font.Bold = true;
				secondCell.Style.Font.FontSize = 13;
				reportSheet.Range("A2:C2").Row(1).Merge();

				reportSheet.Cell("B4").Active = true;

				var summaryRow = reportSheet.ActiveCell.Address.RowNumber;

				var summaryCell = reportSheet.Cell(summaryRow, 2);
				summaryCell.Value = "Прибыль";
				summaryCell.Style.Font.Bold = true;
				summaryCell.Style.Font.FontSize = 13;

				var summaryTotalCell = reportSheet.Cell(summaryRow, 3);
				summaryTotalCell.Style.Font.Bold = true;
				summaryTotalCell.Style.Font.FontSize = 13;

				GoToNextRow(reportSheet);

				GoToNextRow(reportSheet);

				var incomesStartLine = reportSheet.ActiveCell.Address.RowNumber;

				reportSheet.Cell(incomesStartLine, 1).Style.Fill.BackgroundColor = _subTotalsBGColor;
				reportSheet.Cell(incomesStartLine, 2).Style.Fill.BackgroundColor = _subTotalsBGColor;
				reportSheet.Cell(incomesStartLine, 3).Style.Fill.BackgroundColor = _subTotalsBGColor;

				var incomesCell = reportSheet.Cell(incomesStartLine, _mainCategoryColumn);
				incomesCell.Style.Font.Bold = true;
				incomesCell.Value = "Доходы";

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

				var expenseCell = reportSheet.Cell(expensesStartLine, _mainCategoryColumn);
				expenseCell.Style.Font.Bold = true;
				expenseCell.Value = "Расходы";

				reportSheet.Cell(expensesStartLine, 1).Style.Fill.BackgroundColor = _subTotalsBGColor;
				reportSheet.Cell(expensesStartLine, 2).Style.Fill.BackgroundColor = _subTotalsBGColor;
				reportSheet.Cell(expensesStartLine, 3).Style.Fill.BackgroundColor = _subTotalsBGColor;

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

				RenderMoney(reportSheet, summaryRow, cashFlowDdsReport.IncomesGroupLines.Sum(x => x.Money) - cashFlowDdsReport.ExpensesGroupLines.Sum(x => x.Money), true);

				reportSheet.CollapseRows();

				return result;
			}

			private void RenderIncomeGroup(
				IXLWorksheet xLWorksheet,
				CashFlowDdsReport.IncomesGroupLine incomeGroup,
				int groupLevel,
				string numberPrefix)
			{
				var activeAtStartCellRow = xLWorksheet.ActiveCell.Address.RowNumber;

				if(groupLevel > 1)
				{
					RenderGroupTitle(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, incomeGroup.Title, numberPrefix);
				}

				GoToNextRow(xLWorksheet);

				var categoriesEndRowNumber = incomeGroup.IncomeCategories.Count * _incomeTypesCount + incomeGroup.IncomeCategories.Count + activeAtStartCellRow;

				if(incomeGroup.IncomeCategories.Any())
				{
					foreach(var category in incomeGroup.IncomeCategories)
					{
						RenderIncomeCategory(category, xLWorksheet);
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

				var categoriesStartRowNumber = activeAtStartCellRow + 1;

				var operationTypesStartRowNumber = categoriesStartRowNumber + 1;

				if(incomeGroup.Groups.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, groupEndRowNumber).Group();
					}
				}

				if(incomeGroup.IncomeCategories.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, categoriesEndRowNumber).Group();
					}
				}

				RenderMoney(xLWorksheet, activeAtStartCellRow, incomeGroup.Money, true);
			}

			private void RenderExpenseGroup(
				IXLWorksheet xLWorksheet,
				CashFlowDdsReport.ExpensesGroupLine expenseGroup,
				int groupLevel,
				string numberPrefix)
			{
				var activeAtStartCellRow = xLWorksheet.ActiveCell.Address.RowNumber;

				if(groupLevel > 1)
				{
					RenderGroupTitle(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, expenseGroup.Title, numberPrefix);
				}

				GoToNextRow(xLWorksheet);

				var categoriesEndRowNumber = expenseGroup.ExpenseCategories.Count * _expenseTypesCount + expenseGroup.ExpenseCategories.Count + activeAtStartCellRow;

				if(expenseGroup.ExpenseCategories.Any())
				{
					foreach(var category in expenseGroup.ExpenseCategories)
					{
						RenderExpenseCategory(category, xLWorksheet);
					}
				}

				var expenseGroupsCount = expenseGroup.Groups.Count;

				if(expenseGroup.Groups.Any())
				{
					for(int i = 0; i < expenseGroupsCount; i++)
					{
						RenderExpenseGroup(xLWorksheet, expenseGroup.Groups[i], groupLevel + 1, $"{numberPrefix}{i + 1}. ");
					}
				}

				int groupEndRowNumber = xLWorksheet.ActiveCell.Address.RowNumber - 1;

				var categoriesStartRowNumber = activeAtStartCellRow + 1;

				var operationTypesStartRowNumber = categoriesStartRowNumber + 1;

				if(expenseGroup.Groups.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, groupEndRowNumber).Group();
					}
				}

				if(expenseGroup.ExpenseCategories.Any())
				{
					if(groupLevel > 1)
					{
						xLWorksheet.Rows(categoriesStartRowNumber, categoriesEndRowNumber).Group();
					}
				}

				RenderMoney(xLWorksheet, activeAtStartCellRow, expenseGroup.Money, true);
			}

			private void RenderIncomeCategory(
				CashFlowDdsReport.FinancialIncomeCategoryLine category,
				IXLWorksheet xLWorksheet)
			{
				RenderCategoryRow(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, category.Title, category.Money);
				GoToNextRow(xLWorksheet);
				RenderOperationsRows(xLWorksheet, category.OperationsMoney);
			}

			private void RenderExpenseCategory(
				CashFlowDdsReport.FinancialExpenseCategoryLine category,
				IXLWorksheet xLWorksheet)
			{
				RenderCategoryRow(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, category.Title, category.Money);
				GoToNextRow(xLWorksheet);
				RenderOperationsRows(xLWorksheet, category.OperationsMoney);
			}

			private void RenderOperationsRows(IXLWorksheet xLWorksheet, Dictionary<string, decimal> operationsMoney)
			{
				var startRow = xLWorksheet.ActiveCell.Address.RowNumber;

				foreach(var pair in operationsMoney)
				{
					RenderCategoryRow(xLWorksheet, xLWorksheet.ActiveCell.Address.RowNumber, pair.Key, pair.Value, right: true);
					GoToNextRow(xLWorksheet);
				}

				xLWorksheet.Rows(startRow, xLWorksheet.ActiveCell.Address.RowNumber - 1).Group();
			}

			private void GoToNextRow(IXLWorksheet xLWorksheet)
			{
				xLWorksheet.Cell(
					xLWorksheet.ActiveCell.Address.RowNumber + 1,
					xLWorksheet.ActiveCell.Address.ColumnNumber).Active = true;
			}

			private void RenderGroupTitle(IXLWorksheet xLWorksheet, int row, string title, string numberPrefix)
			{
				var leveledCell = xLWorksheet.Cell(row, _categoryTitleColumn);
				leveledCell.Style.Font.Bold = true;
				leveledCell.Value = $"{numberPrefix}{title}";
			}

			private void RenderCategoryRow(IXLWorksheet xLWorksheet, int row, string title, decimal money, bool right = false)
			{
				var titleCell = xLWorksheet.Cell(row, _categoryTitleColumn);

				titleCell.Value = title;

				if(right)
				{
					titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				}

				RenderMoney(xLWorksheet, row, money);
			}

			private void RenderMoney(IXLWorksheet xLWorksheet, int row, decimal money, bool bold = false)
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
