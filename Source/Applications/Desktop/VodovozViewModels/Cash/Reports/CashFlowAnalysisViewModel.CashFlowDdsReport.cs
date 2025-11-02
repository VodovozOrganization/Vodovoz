using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

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
				ReportMode reportMode)
			{
				StartDate = startDate;
				EndDate = endDate;
				IncomesGroupLines = incomesGroupLines;
				ExpensesGroupLines = expensesGroupLines;
				_reportMode = reportMode;
				CreatedAt = DateTime.Now;
			}

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public DateTime CreatedAt { get; }

			public List<IncomesGroupLine> IncomesGroupLines { get; }

			public List<ExpensesGroupLine> ExpensesGroupLines { get; }

			public List<object> Rows => IncomesGroupLines.Cast<object>().Concat(ExpensesGroupLines).ToList();

			public static CashFlowDdsReport GenerateReport(
				IUnitOfWork unitOfWork,
				DateTime startDate,
				DateTime endDate,
				bool hideCategoriesWithoutDocuments,
				ReportMode reportMode)
			{
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
					incomeCategory.Money =
						(from cashIncome in unitOfWork.Session.Query<Income>()
						 where cashIncome.IncomeCategoryId == incomeCategory.Id
							&& cashIncome.Date >= startDate
							&& cashIncome.Date <= endDate
							&& cashIncome.TypeOperation != IncomeType.Return
						 select cashIncome.Money)
						.ToArray()
						.Sum();
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
					var expensesSum =
						(from cashExpense in unitOfWork.Session.Query<Expense>()
						 where cashExpense.ExpenseCategoryId == expenseCategory.Id
							&& ((reportMode == ReportMode.Dds 
									&& cashExpense.Date >= startDate
									&& cashExpense.Date <= endDate)
								|| (reportMode == ReportMode.Ddr
									&& cashExpense.DdrDate >= startDate
									&& cashExpense.DdrDate <= endDate))
						 select cashExpense.Money)
						.ToArray()
						.Sum();

					expensesSum -=
						(from cashIncome in unitOfWork.Session.Query<Income>()
						 where cashIncome.ExpenseCategoryId == expenseCategory.Id
							&& cashIncome.Date >= startDate
							&& cashIncome.Date <= endDate
							&& cashIncome.TypeOperation == IncomeType.Return
						 select cashIncome.Money)
						.ToArray()
						.Sum();

					expenseCategory.Money = expensesSum;
				}

				if(hideCategoriesWithoutDocuments)
				{
					expensesCategories.RemoveAll(x => x.Money == 0);
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

				var expenseGroupLines = ProceedExpenseGroups(null, expenseGroups, expensesCategories, hideCategoriesWithoutDocuments);

				return new CashFlowDdsReport(
					startDate,
					endDate,
					incomeGroupLines,
					expenseGroupLines,
					reportMode);
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
