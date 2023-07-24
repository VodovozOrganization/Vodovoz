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
			public string Title => "Анализ движения денежных средств";

			private CashFlowDdsReport(
				DateTime startDate,
				DateTime endDate,
				List<IncomesGroupLine> incomesGroupLines,
				List<ExpensesGroupLine> expensesGroupLines)
			{
				StartDate = startDate;
				EndDate = endDate;
				IncomesGroupLines = incomesGroupLines;
				ExpensesGroupLines = expensesGroupLines;

				CreatedAt = DateTime.Now;
			}

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public DateTime CreatedAt { get; }

			public List<IncomesGroupLine> IncomesGroupLines { get; }

			public List<ExpensesGroupLine> ExpensesGroupLines { get; }

			public List<object> Rows => IncomesGroupLines.Cast<object>().Concat(ExpensesGroupLines).ToList();

			public static CashFlowDdsReport GenerateReport(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate)
			{
				var incomesCategories = (from incomeCategory in unitOfWork.Session.Query<FinancialIncomeCategory>()
										 where incomeCategory.GroupType == GroupType.Category
											&& incomeCategory.IsArchive == false
											&& incomeCategory.ExcludeFromCashFlowDds == false
										 select FinancialIncomeCategoryLine.Create(
											  incomeCategory.Id,
											  incomeCategory.ParentId,
											  incomeCategory.Title))
										.ToList();

				foreach(var incomeCategory in incomesCategories)
				{
					incomeCategory.Money = (from cashIncome in unitOfWork.Session.Query<Income>()
											where cashIncome.IncomeCategoryId == incomeCategory.Id
											   && cashIncome.Date >= startDate
											   && cashIncome.Date <= endDate
											   && cashIncome.TypeOperation != IncomeType.Return
											select cashIncome.Money)
											.ToArray()
											.Sum();
				}

				var expensesCategories = (from expenseCategory in unitOfWork.Session.Query<FinancialExpenseCategory>()
										  where expenseCategory.GroupType == GroupType.Category
											 && expenseCategory.IsArchive == false
											 && expenseCategory.ExcludeFromCashFlowDds == false
										  select FinancialExpenseCategoryLine.Create(
											  expenseCategory.Id,
											  expenseCategory.ParentId,
											  expenseCategory.Title))
											 .ToList();

				foreach(var expenseCategory in expensesCategories)
				{
					var expensesSum = (from cashExpense in unitOfWork.Session.Query<Expense>()
									   where cashExpense.ExpenseCategoryId == expenseCategory.Id
										  && cashExpense.Date >= startDate
										  && cashExpense.Date <= endDate
									   select cashExpense.Money)
									   .ToArray()
									   .Sum();

					expensesSum -= (from cashIncome in unitOfWork.Session.Query<Income>()
									where cashIncome.ExpenseCategoryId == expenseCategory.Id
									   && cashIncome.Date >= startDate
									   && cashIncome.Date <= endDate
									   && cashIncome.TypeOperation == IncomeType.Return
									select cashIncome.Money)
									.ToArray()
									.Sum();

					expenseCategory.Money = expensesSum;
				}

				var incomeGroups = (from incomeGroup in unitOfWork.Session.Query<FinancialCategoriesGroup>()
									where incomeGroup.FinancialSubtype == FinancialSubType.Income
									   && incomeGroup.GroupType == GroupType.Group
									   && incomeGroup.IsArchive == false
									select incomeGroup)
									.ToList();

				var incomeGroupLines = ProceedIncomeGroups(null, incomeGroups, incomesCategories);

				var expenseGroups = (from expenseGroup in unitOfWork.Session.Query<FinancialCategoriesGroup>()
									 where expenseGroup.FinancialSubtype == FinancialSubType.Expense
										&& expenseGroup.GroupType == GroupType.Group
										&& expenseGroup.IsArchive == false
									 select expenseGroup)
									.ToList();

				var expenseGroupLines = ProceedExpenseGroups(null, expenseGroups, expensesCategories);

				return new CashFlowDdsReport(
					startDate,
					endDate,
					incomeGroupLines,
					expenseGroupLines);
			}

			private static List<IncomesGroupLine> ProceedIncomeGroups(
				int? parentId,
				List<FinancialCategoriesGroup> groups,
				List<FinancialIncomeCategoryLine> financialIncomeCategoryLines)
			{
				var result = new List<IncomesGroupLine>();

				var groupsToProceed = groups.Where(x => x.ParentId == parentId).ToList();

				foreach(var group in groupsToProceed)
				{
					result.Add(IncomesGroupLine.Create(
						group.Id,
						group.Title,
						ProceedIncomeGroups(group.Id, groups, financialIncomeCategoryLines),
						financialIncomeCategoryLines.Where(x => x.ParentId == group.Id).ToList()));
				}

				return result;
			}

			private static List<ExpensesGroupLine> ProceedExpenseGroups(
				int? parentId,
				List<FinancialCategoriesGroup> groups,
				List<FinancialExpenseCategoryLine> financialExpenseCategoryLines)
			{
				var result = new List<ExpensesGroupLine>();

				var groupsToProceed = groups.Where(x => x.ParentId == parentId).ToList();

				foreach(var group in groupsToProceed)
				{
					result.Add(ExpensesGroupLine.Create(
						group.Id,
						group.Title,
						ProceedExpenseGroups(group.Id, groups, financialExpenseCategoryLines),
						financialExpenseCategoryLines.Where(x => x.ParentId == group.Id).ToList()));
				}

				return result;
			}
		}
	}
}
