namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			public class FinancialIncomeCategoryLine
			{
				public FinancialIncomeCategoryLine(int id, int? parentId, string title)
				{
					Id = id;
					ParentId = parentId;
					Title = title;
				}

				public int Id { get; }

				public int? ParentId { get; }

				public string Title { get; }

				public decimal Money { get; set; }

				public static FinancialIncomeCategoryLine Create(int id, int? parentId, string title) => new FinancialIncomeCategoryLine(id, parentId, title);
			}
		}
	}
}
