namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			public class FinancialExpenseCategoryLine
			{
				public FinancialExpenseCategoryLine(int id, int? parentId, string title, string numbering)
				{
					Id = id;
					ParentId = parentId;
					Title = title;
					Numbering = numbering;
				}

				public int Id { get; }

				public int? ParentId { get; }

				public string Numbering { get; }

				public string Title { get; }

				public decimal Money { get; set; }

				public static FinancialExpenseCategoryLine Create(int id, int? parentId, string title, string numbering) =>
					new FinancialExpenseCategoryLine(id, parentId, title, numbering);
			}
		}
	}
}
