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
				CashFlowReportLine incomeSection,
				CashFlowReportLine expenseSection,
				ReportMode reportMode)
			{
				StartDate = startDate;
				EndDate = endDate;
				IncomeSection = incomeSection;
				ExpenseSection = expenseSection;
				ProfitLine = new CashFlowReportLine("Прибыль", incomeSection.Money - expenseSection.Money);
				_reportMode = reportMode;
				CreatedAt = DateTime.Now;
			}

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public DateTime CreatedAt { get; }

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
			/// <param name="hideCategoriesWithoutDocuments">Скрывать конечные строки с нулевыми суммами</param>
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

				var incomeArticles =
					(from article in unitOfWork.Session.Query<FinancialIncomeCategory>()
					 where article.GroupType == GroupType.Category && !article.IsArchive && !article.ExcludeFromCashFlowDds
					 orderby article.Numbering, article.Title
					 select new ReportArticle
					 {
						Id = article.Id,
						ParentId = article.ParentId,
						Title = article.Title,
						Numbering = article.Numbering
					 })
					.ToList();

				var expenseArticles =
					(from article in unitOfWork.Session.Query<FinancialExpenseCategory>()
					 where article.GroupType == GroupType.Category && !article.IsArchive && !article.ExcludeFromCashFlowDds
					 orderby article.Numbering, article.Title
					 select new ReportArticle
					 {
						Id = article.Id,
						ParentId = article.ParentId,
						Title = article.Title,
						Numbering = article.Numbering
					 })
					.ToList();

				var groups = unitOfWork.Session.Query<FinancialCategoriesGroup>()
					.Where(x => x.GroupType == GroupType.Group && !x.IsArchive)
					.OrderBy(x => x.Numbering)
					.ThenBy(x => x.Title)
					.ToList()
					.ToLookup(x => x.FinancialSubtype);

				var incomeArticleLines = BuildArticleLines(
					groups[FinancialSubType.Income], incomeArticles, data.IncomeTotals, hideCategoriesWithoutDocuments);
				var incomeLines = !includeCenters || groupings.Contains(CashFlowReportGrouping.Article)
					? incomeArticleLines
					: Array.Empty<CashFlowReportLine>();

				// Каталог для центров включает статьи с нулевым итогом по всем подразделениям
				var expenseArticleLines = BuildArticleLines(
					groups[FinancialSubType.Expense], expenseArticles, expenseTotals, hideCategoriesWithoutDocuments && !includeCenters);
				var expenseLines = includeCenters
					? CashFlowReportExpenseTree.Build(
						groupings, expenseArticleLines, expenseMovements, data.SubdivisionNames, hideCategoriesWithoutDocuments)
					: expenseArticleLines;

				return new CashFlowDdsReport(
					startDate,
					endDate,
					new CashFlowReportLine("Доходы", incomeArticleLines.Sum(x => x.Money), incomeLines),
					new CashFlowReportLine("Расходы", expenseLines.Sum(x => x.Money), expenseLines),
					reportMode);
			}

			private static CashFlowReportLine[] BuildArticleLines(
				IEnumerable<FinancialCategoriesGroup> groups,
				IEnumerable<ReportArticle> articles,
				IReadOnlyDictionary<int, decimal> totals,
				bool hideZeroLeaves)
			{
				var groupsByParent = groups.ToLookup(x => x.ParentId);
				var articlesByParent = articles.ToLookup(x => x.ParentId);

				return groupsByParent[null]
					.SelectMany(root => BuildArticleLines(root.Id, groupsByParent, articlesByParent, totals, hideZeroLeaves))
					.ToArray();
			}

			private static IEnumerable<CashFlowReportLine> BuildArticleLines(
				int parentId,
				ILookup<int?, FinancialCategoriesGroup> groups,
				ILookup<int?, ReportArticle> articles,
				IReadOnlyDictionary<int, decimal> totals,
				bool hideZeroLeaves)
			{
				foreach(var group in groups[parentId])
				{
					var children = BuildArticleLines(group.Id, groups, articles, totals, hideZeroLeaves).ToArray();

					if(!hideZeroLeaves || children.Length > 0)
					{
						yield return new CashFlowReportLine(group.Title, children.Sum(x => x.Money), children, group.Id, group.Numbering);
					}
				}

				foreach(var article in articles[parentId])
				{
					totals.TryGetValue(article.Id, out var money);

					if(!hideZeroLeaves || money != 0)
					{
						yield return new CashFlowReportLine(article.Title, money, id: article.Id, numbering: article.Numbering, isArticle: true);
					}
				}
			}

			private sealed class ReportArticle
			{
				public int Id { get; set; }
				public int? ParentId { get; set; }
				public string Title { get; set; }
				public string Numbering { get; set; }
			}
		}
	}
}
