using System.Collections.Generic;
using Vodovoz.Models.Cash;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel;

namespace Vodovoz.Cash.Reports
{
	public partial class CashFlowAnalysisView
	{
		internal sealed class ReportLine
		{
			private ReportLine(CashFlowReportLine line, ReportLine parent)
			{
				ParentLine = parent;
				FirstColumn = parent == null ? line.Title : line.Numbering;
				SecondColumn = parent == null ? null : line.Title;
				ThirdColumn = line.Money;
				IsAccented = parent == null;
				Bold = !line.IsArticle || line.Children.Count > 0;

				foreach(var child in line.Children)
				{
					ChildLines.Add(new ReportLine(child, this));
				}
			}

			public ReportLine ParentLine { get; }

			public List<ReportLine> ChildLines { get; } = new List<ReportLine>();

			public string FirstColumn { get; }

			public string SecondColumn { get; }

			public decimal ThirdColumn { get; }

			public bool IsAccented { get; }

			public bool Bold { get; }

			public static List<ReportLine> Map(CashFlowDdsReport report)
			{
				return new List<ReportLine>
				{
					new ReportLine(report.IncomeSection, null),
					new ReportLine(report.ExpenseSection, null),
					new ReportLine(report.ProfitLine, null)
				};
			}
		}
	}
}
