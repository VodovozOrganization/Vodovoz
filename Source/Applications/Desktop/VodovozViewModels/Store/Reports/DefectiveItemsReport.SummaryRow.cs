using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class SummaryRow
		{
			public string DefectType { get; set; }
			public IEnumerable<decimal> DynamicColls { get; set; }
			public decimal Summary { get; set; }
		}
	}
}
