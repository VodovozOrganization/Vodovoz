using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			public class ExpensesGroupLine
			{
				private ExpensesGroupLine(
					int id,
					string title,
					string numbering,
					List<ExpensesGroupLine> groups,
					List<FinancialExpenseCategoryLine> incomeCategories)
				{
					Id = id;
					Title = title;
					Numbering = numbering;
					Groups = groups;
					ExpenseCategories = incomeCategories;
				}

				public int Id { get; }

				public string Numbering { get; }

				public string Title { get; }

				public List<ExpensesGroupLine> Groups { get; }

				public List<FinancialExpenseCategoryLine> ExpenseCategories { get; }

				public decimal Money => ExpenseCategories.Sum(x => x.Money) + Groups.Sum(x => x.Money);

				public static ExpensesGroupLine Create(
					int id,
					string title,
					string numbering,
					List<ExpensesGroupLine> groups,
					List<FinancialExpenseCategoryLine> incomeCategories)
						=> new ExpensesGroupLine(id, title, numbering, groups, incomeCategories);
			}
		}
	}
}
