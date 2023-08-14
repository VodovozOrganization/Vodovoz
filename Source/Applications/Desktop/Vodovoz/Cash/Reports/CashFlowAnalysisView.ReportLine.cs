using NHibernate.Util;
using System.Collections.Generic;
using System.Linq;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel.CashFlowDdsReport;

namespace Vodovoz.Cash.Reports
{
	public partial class CashFlowAnalysisView
	{
		internal class ReportLine
		{
			private ReportLine()
			{

			}

			public ReportLine ParentLine { get; set; } = null;

			public List<ReportLine> ChildLines { get; } = new List<ReportLine>();

			public string FirstColumn { get; set; }

			public string SecondColumn { get; set; }

			public decimal ThirdColumn { get; set; }

			public bool IsAccented { get; set; } = false;

			public bool Bold { get; set; }

			public bool IsSeparator { get; private set; } = false;

			public static List<ReportLine> Map(CashFlowDdsReport report)
			{
				var result = new List<ReportLine>
				{
					Map(report.IncomesGroupLines.First()),
					Map(report.ExpensesGroupLines.First()),
					new ReportLine()
					{
						FirstColumn = "Прибыль",
						ThirdColumn = report.IncomesGroupLines.Sum(x => x.Money) - report.ExpensesGroupLines.Sum(x => x.Money),
						Bold = true,
						IsAccented = true
					}
				};

				return result;
			}

			private static ReportLine Map(IncomesGroupLine incomesGroupLine)
			{
				var result = new ReportLine
				{
					FirstColumn = "Доходы",
					ThirdColumn = incomesGroupLine.Money,
					Bold = true,
					IsAccented = true
				};

				result.ChildLines.AddRange(Map(
						result,
						incomesGroupLine.Groups,
						incomesGroupLine.IncomeCategories));

				return result;
			}

			private static ReportLine Map(ExpensesGroupLine expensesGroupLine)
			{
				var result = new ReportLine
				{
					FirstColumn = "Расходы",
					ThirdColumn = expensesGroupLine.Money,
					Bold = true,
					IsAccented = true
				};

				result.ChildLines.AddRange(Map(
					result,
					expensesGroupLine.Groups,
					expensesGroupLine.ExpenseCategories));

				return result;
			}

			private static IEnumerable<ReportLine> Map(
				ReportLine parent,
				List<ExpensesGroupLine> expensesGroupLines,
				List<FinancialExpenseCategoryLine> expenseCategoryLines)
			{
				var result = new List<ReportLine>();

				foreach(var expensesGroupLine in expensesGroupLines)
				{
					result.Add(Map(parent, expensesGroupLine));
				}

				foreach(var expenseCategoryLine in expenseCategoryLines)
				{
					result.Add(Map(parent, expenseCategoryLine));
				}

				return result;
			}

			private static ReportLine Map(
				ReportLine parent,
				FinancialExpenseCategoryLine expenseCategoryLine)
			{
				return new ReportLine()
				{
					ParentLine = parent,
					FirstColumn = expenseCategoryLine.Numbering,
					SecondColumn = expenseCategoryLine.Title,
					ThirdColumn = expenseCategoryLine.Money
				};
			}

			private static ReportLine Map(
				ReportLine parent,
				ExpensesGroupLine expensesGroupLine)
			{
				var result = new ReportLine()
				{
					Bold = true,
					ParentLine = parent,
					FirstColumn = expensesGroupLine.Numbering,
					SecondColumn = expensesGroupLine.Title,
					ThirdColumn = expensesGroupLine.Money
				};

				result.ChildLines.AddRange(Map(
					result,
					expensesGroupLine.Groups,
					expensesGroupLine.ExpenseCategories));

				return result;
			}

			private static List<ReportLine> Map(
				ReportLine parent,
				IEnumerable<IncomesGroupLine> incomesGroupLines,
				IEnumerable<FinancialIncomeCategoryLine> incomeCategoryLines)
			{
				var result = new List<ReportLine>();

				foreach(var incomesGroupLine in incomesGroupLines)
				{
					result.Add(Map(parent, incomesGroupLine));
				}

				foreach(var incomeCategoryLine in incomeCategoryLines)
				{
					result.Add(Map(parent, incomeCategoryLine));
				}

				return result;
			}

			private static ReportLine Map(
				ReportLine parent,
				FinancialIncomeCategoryLine incomeCategoryLine)
			{
				return new ReportLine()
				{
					ParentLine = parent,
					FirstColumn = incomeCategoryLine.Numbering,
					SecondColumn = incomeCategoryLine.Title,
					ThirdColumn = incomeCategoryLine.Money
				};
			}

			private static ReportLine Map(
				ReportLine parent,
				IncomesGroupLine incomesGroupLine)
			{
				var result = new ReportLine()
				{
					Bold = true,
					ParentLine = parent,
					FirstColumn = incomesGroupLine.Numbering,
					SecondColumn = incomesGroupLine.Title,
					ThirdColumn = incomesGroupLine.Money
				};

				result.ChildLines.AddRange(Map(
					result,
					incomesGroupLine.Groups,
					incomesGroupLine.IncomeCategories));

				return result;
			}
		}
	}
}
