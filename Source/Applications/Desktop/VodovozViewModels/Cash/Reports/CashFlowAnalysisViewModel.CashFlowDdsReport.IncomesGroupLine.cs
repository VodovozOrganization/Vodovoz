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
					string numbering,
					List<IncomesGroupLine> groups,
					List<FinancialIncomeCategoryLine> incomeCategories)
				{
					Id = id;
					Title = title;
					Numbering = numbering;
					Groups = groups;
					IncomeCategories = incomeCategories;
				}

				public int Id { get; }

				public string Numbering { get; }

				public string Title { get; }

				public List<IncomesGroupLine> Groups { get; }

				public List<FinancialIncomeCategoryLine> IncomeCategories { get; }

				public decimal Money => IncomeCategories.Sum(x => x.Money) + Groups.Sum(x => x.Money);

				public static IncomesGroupLine Create(
					int id,
					string title,
					string numbering,
					List<IncomesGroupLine> groups,
					List<FinancialIncomeCategoryLine> incomeCategories)
						=> new IncomesGroupLine(id, title, numbering, groups, incomeCategories);
			}
		}
	}
}
