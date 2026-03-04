using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingSummaryReport
	{
		public IList<BulkDebtMailingSummaryReportRow> Rows { get; set; }
		public string SelectedFilters { get; set; }
	}
}
