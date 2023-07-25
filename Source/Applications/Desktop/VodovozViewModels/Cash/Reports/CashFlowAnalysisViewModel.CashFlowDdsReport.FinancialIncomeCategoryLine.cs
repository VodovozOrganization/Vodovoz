namespace Vodovoz.ViewModels.Cash.Reports
{
	public partial class CashFlowAnalysisViewModel
	{
		public partial class CashFlowDdsReport
		{
			public class FinancialIncomeCategoryLine
			{
				public FinancialIncomeCategoryLine(int id, int? parentId, string title, string numbering)
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

				public static FinancialIncomeCategoryLine Create(int id, int? parentId, string title, string numbering) =>
					new FinancialIncomeCategoryLine(id, parentId, title, numbering);
			}
		}
	}
}
