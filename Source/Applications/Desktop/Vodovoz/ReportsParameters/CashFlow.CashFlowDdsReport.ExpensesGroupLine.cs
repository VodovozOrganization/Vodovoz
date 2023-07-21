using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		public partial class CashFlowDdsReport
		{
			public class ExpensesGroupLine
			{
				private ExpensesGroupLine(
					int id,
					string title,
					List<ExpensesGroupLine> groups,
					List<FinancialExpenseCategoryLine> incomeCategories)
				{
					Id = id;
					Title = title;
					Groups = groups;
					ExpenseCategories = incomeCategories;
				}

				public int Id { get; }

				public string Title { get; }

				public List<ExpensesGroupLine> Groups { get; }

				public List<FinancialExpenseCategoryLine> ExpenseCategories { get; }

				public decimal Money => ExpenseCategories.Sum(x => x.Money) + Groups.Sum(x => x.Money);

				public static ExpensesGroupLine Create(
					int id,
					string title,
					List<ExpensesGroupLine> groups,
					List<FinancialExpenseCategoryLine> incomeCategories)
						=> new ExpensesGroupLine(id, title, groups, incomeCategories);
			}
		}
	}
}
