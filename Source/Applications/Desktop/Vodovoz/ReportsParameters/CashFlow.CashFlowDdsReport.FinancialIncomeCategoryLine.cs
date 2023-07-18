namespace Vodovoz.Reports
{
	public partial class CashFlow
	{
		public partial class CashFlowDdsReport
		{
			public class FinancialIncomeCategoryLine
			{
				public FinancialIncomeCategoryLine(int id, int? parentId, string title, decimal money)
				{
					Id = id;
					ParentId = parentId;
					Title = title;
					Money = money;
				}

				public int Id { get; }

				public int? ParentId { get; }

				public string Title { get; }

				public decimal Money { get; }

				public static FinancialIncomeCategoryLine Create(int id, int? parentId, string title, decimal money) => new FinancialIncomeCategoryLine(id, parentId, title, money);
			}
		}
	}
}
