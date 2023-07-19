using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Reports
{
	public partial class CashFlow
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
					OperationsMoney = new Dictionary<string, decimal>();
				}

				public int Id { get; }

				public int? ParentId { get; }

				public string Title { get; }

				public Dictionary<string, decimal> OperationsMoney { get; set; }

				public decimal Money => OperationsMoney.Sum(x => x.Value);

				public static FinancialIncomeCategoryLine Create(int id, int? parentId, string title) => new FinancialIncomeCategoryLine(id, parentId, title);
			}
		}
	}
}
