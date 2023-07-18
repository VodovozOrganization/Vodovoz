namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		public partial class CashFlowDdsReport
		{
			public class FinancialExpenseCategoryLine
			{
				public FinancialExpenseCategoryLine(int id, int? parentId, string title)
				{
					Id = id;
					ParentId = parentId;
					Title = title;
				}

				public int Id { get; }

				public int? ParentId { get; }

				public string Title { get; }

				public decimal Money { get; set; }

				public static FinancialExpenseCategoryLine Create(int id, int? parentId, string title) => new FinancialExpenseCategoryLine(id, parentId, title);
			}
		}
	}
}
