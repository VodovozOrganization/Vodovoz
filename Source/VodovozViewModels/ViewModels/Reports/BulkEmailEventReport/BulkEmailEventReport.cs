using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport
{
	public class BulkEmailEventReport
	{
		public IList<BulkEmailEventReportRow> Rows { get; set; }
		public string SelectedFilters { get; set; }
	}
}
