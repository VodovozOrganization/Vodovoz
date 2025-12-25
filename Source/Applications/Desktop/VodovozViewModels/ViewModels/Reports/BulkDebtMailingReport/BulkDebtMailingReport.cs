using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingReport
	{
		public IList<BulkDebtMailingReportRow> Rows { get; set; }
		public string SelectedFilters { get; set; }
	}
}
