using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class SummaryDisplayRow
		{
			public string Title { get; set; }
			public IEnumerable<string> DynamicColls { get; set; }
			public string Summary { get; set; }
		}
	}
}
