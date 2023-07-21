using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			public class IncomesGroupLine
			{
				private IncomesGroupLine(
					int id,
					string title,
					List<IncomesGroupLine> groups,
					List<FinancialIncomeCategoryLine> incomeCategories)
				{
					Id = id;
					Title = title;
					Groups = groups;
					IncomeCategories = incomeCategories;
				}

				public int Id { get; }

				public string Title { get; }

				public List<IncomesGroupLine> Groups { get; }

				public List<FinancialIncomeCategoryLine> IncomeCategories { get; }

				public decimal Money => IncomeCategories.Sum(x => x.Money) + Groups.Sum(x => x.Money);

				public static IncomesGroupLine Create(
					int id,
					string title,
					List<IncomesGroupLine> groups,
					List<FinancialIncomeCategoryLine> incomeCategories)
						=> new IncomesGroupLine(id, title, groups, incomeCategories);
			}
		}
	}
}
