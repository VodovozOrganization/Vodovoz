using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.Cash;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			private readonly ReportMode _reportMode;

			public string Title => _reportMode == ReportMode.Dds ? "Анализ движения денежных средств" : "Анализ движения денежных расходов";

			private CashFlowDdsReport(
				DateTime startDate,
				DateTime endDate,
				List<IncomesGroupLine> incomesGroupLines,
				List<ExpensesGroupLine> expensesGroupLines,
				CashFlowReportLine incomeSection,
				CashFlowReportLine expenseSection,
				ReportMode reportMode)
			{
				StartDate = startDate;
				EndDate = endDate;
				IncomesGroupLines = incomesGroupLines;
				ExpensesGroupLines = expensesGroupLines;
				IncomeSection = incomeSection;
				ExpenseSection = expenseSection;
				ProfitLine = new CashFlowReportLine("Прибыль", incomeSection.Money - expenseSection.Money);
				_reportMode = reportMode;
				CreatedAt = DateTime.Now;
			}

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public DateTime CreatedAt { get; }

			public List<IncomesGroupLine> IncomesGroupLines { get; }

			public List<ExpensesGroupLine> ExpensesGroupLines { get; }

			public List<object> Rows => IncomesGroupLines.Cast<object>().Concat(ExpensesGroupLines).ToList();

			/// <summary>
			/// Доходы с выбранной детализацией
			/// </summary>
			public CashFlowReportLine IncomeSection { get; }

			/// <summary>
			/// Чистые расходы с выбранной детализацией
			/// </summary>
			public CashFlowReportLine ExpenseSection { get; }

			/// <summary>
			/// Итоговая разница доходов и расходов
			/// </summary>
			public CashFlowReportLine ProfitLine { get; }

			/// <summary>
			/// Формирует отчёт по кассовым движениям за период
			/// </summary>
			/// <param name="unitOfWork">UnitOfWork</param>
			/// <param name="cashRepository">Репозиторий кассовых документов</param>
			/// <param name="startDate">Нижняя граница периода</param>
			/// <param name="endDate">Верхняя граница периода</param>
			/// <param name="hideCategoriesWithoutDocuments">Скрывать статьи с нулевыми суммами</param>
			/// <param name="reportMode">Режим ДДС или ДДР</param>
			/// <param name="groupings">Выбранные группировки в порядке вложенности</param>
			/// <returns>Сформированный отчёт</returns>
			public static CashFlowDdsReport GenerateReport(
				IUnitOfWork unitOfWork,
				ICashRepository cashRepository,
				DateTime startDate,
				DateTime endDate,
				bool hideCategoriesWithoutDocuments,
				ReportMode reportMode,
				IReadOnlyList<CashFlowReportGrouping> groupings)
			{
				var includeCenters = groupings.Any(x => x != CashFlowReportGrouping.Article);
				var data = cashRepository.GetCashFlowReportData(
					unitOfWork, startDate, endDate, useDdrDate: reportMode == ReportMode.Ddr, includeCenters: includeCenters);
				var returnedExpenses = CashFlowReportReturnAllocation.Allocate(data.Returns, data.DirectClosings, data.AdvanceReportClosings);
				var expenseMovements = data.Expenses.Concat(returnedExpenses).ToArray();
				var expenseTotals = expenseMovements
					.GroupBy(x => x.ExpenseCategoryId)
					.ToDictionary(x => x.Key, x => x.Sum(amount => amount.Money));

				var incomesCategories =
					(from incomeCategory in unitOfWork.Session.Query<FinancialIncomeCategory>()
					 where incomeCategory.GroupType == GroupType.Category
						&& incomeCategory.IsArchive == false
						&& incomeCategory.ExcludeFromCashFlowDds == false
					 orderby incomeCategory.Numbering, incomeCategory.Title
					 select FinancialIncomeCategoryLine.Create(
						  incomeCategory.Id,
						  incomeCategory.ParentId,
						  incomeCategory.Title,
						  incomeCategory.Numbering))
					.ToList();

				foreach(var incomeCategory in incomesCategories)
				{
					data.IncomeTotals.TryGetValue(incomeCategory.Id, out var incomeTotal);
					incomeCategory.Money = incomeTotal;
				}

				if(hideCategoriesWithoutDocuments)
				{
					incomesCategories.RemoveAll(x => x.Money == 0);
				}

				var expensesCategories =
					(from expenseCategory in unitOfWork.Session.Query<FinancialExpenseCategory>()
					 where expenseCategory.GroupType == GroupType.Category
						&& expenseCategory.IsArchive == false
						&& expenseCategory.ExcludeFromCashFlowDds == false
					 orderby expenseCategory.Numbering, expenseCategory.Title
					 select FinancialExpenseCategoryLine.Create(
						 expenseCategory.Id,
						 expenseCategory.ParentId,
						 expenseCategory.Title,
						 expenseCategory.Numbering))
					 .ToList();

				foreach(var expenseCategory in expensesCategories)
				{
					expenseTotals.TryGetValue(expenseCategory.Id, out var expenseTotal);
					expenseCategory.Money = expenseTotal;
				}

				var incomeGroups =
					(from incomeGroup in unitOfWork.Session.Query<FinancialCategoriesGroup>()
					 where incomeGroup.FinancialSubtype == FinancialSubType.Income
						&& incomeGroup.GroupType == GroupType.Group
						&& incomeGroup.IsArchive == false
					 orderby incomeGroup.Numbering, incomeGroup.Title
					 select incomeGroup)
					.ToList();

				var incomeGroupLines = ProceedIncomeGroups(null, incomeGroups, incomesCategories, hideCategoriesWithoutDocuments);

				var expenseGroups =
					(from expenseGroup in unitOfWork.Session.Query<FinancialCategoriesGroup>()
					 where expenseGroup.FinancialSubtype == FinancialSubType.Expense
						&& expenseGroup.GroupType == GroupType.Group
						&& expenseGroup.IsArchive == false
					 orderby expenseGroup.Numbering, expenseGroup.Title
					 select expenseGroup)
					.ToList();

				// Каталог для центров включает статьи с нулевым итогом по всем подразделениям
				var expenseCatalogGroups = includeCenters && hideCategoriesWithoutDocuments
					? ProceedExpenseGroups(null, expenseGroups, expensesCategories, false)
					: null;

				if(hideCategoriesWithoutDocuments)
				{
					expensesCategories.RemoveAll(x => x.Money == 0);
				}

				var expenseGroupLines = ProceedExpenseGroups(null, expenseGroups, expensesCategories, hideCategoriesWithoutDocuments);
				var incomeLines = !includeCenters || groupings.Contains(CashFlowReportGrouping.Article)
					? incomeGroupLines.SelectMany(GetIncomeArticleLines).ToArray()
					: Array.Empty<CashFlowReportLine>();
				var expenseArticleLines = (expenseCatalogGroups ?? expenseGroupLines).SelectMany(GetExpenseArticleLines).ToArray();
				var expenseLines = includeCenters
					? CashFlowReportExpenseTree.Build(
						groupings, expenseArticleLines, expenseMovements, data.SubdivisionNames, hideCategoriesWithoutDocuments)
					: expenseArticleLines;

				return new CashFlowDdsReport(
					startDate,
					endDate,
					incomeGroupLines,
					expenseGroupLines,
					new CashFlowReportLine("Доходы", incomeGroupLines.Sum(x => x.Money), incomeLines),
					new CashFlowReportLine("Расходы", expenseLines.Sum(x => x.Money), expenseLines),
					reportMode);
			}

			private static IEnumerable<CashFlowReportLine> GetIncomeArticleLines(IncomesGroupLine root)
			{
				foreach(var group in root.Groups)
				{
					var children = GetIncomeArticleLines(group).ToArray();

					yield return new CashFlowReportLine(group.Title, children.Sum(x => x.Money), children, group.Id, group.Numbering);
				}

				foreach(var article in root.IncomeCategories)
				{
					yield return new CashFlowReportLine(article.Title, article.Money, id: article.Id, numbering: article.Numbering, isArticle: true);
				}
			}

			private static IEnumerable<CashFlowReportLine> GetExpenseArticleLines(ExpensesGroupLine root)
			{
				foreach(var group in root.Groups)
				{
					var children = GetExpenseArticleLines(group).ToArray();

					yield return new CashFlowReportLine(group.Title, children.Sum(x => x.Money), children, group.Id, group.Numbering);
				}

				foreach(var article in root.ExpenseCategories)
				{
					yield return new CashFlowReportLine(article.Title, article.Money, id: article.Id, numbering: article.Numbering, isArticle: true);
				}
			}

			private static List<IncomesGroupLine> ProceedIncomeGroups(
				int? parentId,
				List<FinancialCategoriesGroup> groups,
				List<FinancialIncomeCategoryLine> financialIncomeCategoryLines,
				bool hideCategoriesWithoutDocuments)
			{
				var result = new List<IncomesGroupLine>();

				var groupsToProceed = groups.Where(x => x.ParentId == parentId).ToList();

				foreach(var group in groupsToProceed)
				{
					var subgroups = ProceedIncomeGroups(group.Id, groups, financialIncomeCategoryLines, hideCategoriesWithoutDocuments);

					var subcategories = financialIncomeCategoryLines.Where(x => x.ParentId == group.Id).ToList();

					if(!hideCategoriesWithoutDocuments || subcategories.Any() || subgroups.Any())
					{
						result.Add(IncomesGroupLine.Create(
							group.Id,
							group.Title,
							group.Numbering,
							subgroups,
							subcategories));
					}
				}

				return result;
			}

			private static List<ExpensesGroupLine> ProceedExpenseGroups(
				int? parentId,
				List<FinancialCategoriesGroup> groups,
				List<FinancialExpenseCategoryLine> financialExpenseCategoryLines,
				bool hideCategoriesWithoutDocuments)
			{
				var result = new List<ExpensesGroupLine>();

				var groupsToProceed = groups.Where(x => x.ParentId == parentId).ToList();

				foreach(var group in groupsToProceed)
				{
					var subgroups = ProceedExpenseGroups(group.Id, groups, financialExpenseCategoryLines, hideCategoriesWithoutDocuments);

					var subcategories = financialExpenseCategoryLines.Where(x => x.ParentId == group.Id).ToList();

					if(!hideCategoriesWithoutDocuments || subcategories.Any() || subgroups.Any())
					{
						result.Add(ExpensesGroupLine.Create(
							group.Id,
							group.Title,
							group.Numbering,
							subgroups,
							subcategories));
					}
				}

				return result;
			}
		}
	}
}
