using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		private enum ReportParts
		{
			[Display(Name = "Поступления суммарно")]
			IncomeAll,
			[Display(Name = "Приход")]
			Income,
			[Display(Name = "Сдача")]
			IncomeReturn,
			[Display(Name = "Расходы суммарно")]
			ExpenseAll,
			[Display(Name = "Расход")]
			Expense,
			[Display(Name = "Авансы")]
			Advance,
			[Display(Name = "Авансовые отчеты")]
			AdvanceReport,
			[Display(Name = "Незакрытые авансы")]
			UnclosedAdvance
		}
	}
}
