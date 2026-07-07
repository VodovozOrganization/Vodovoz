using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public partial class MarketingReport
		{
			public class DisplayRow
			{
				public virtual string Title { get; set; }
				public virtual IList<string> DynamicColumns { get; set; }
			}
		}
	}
}
