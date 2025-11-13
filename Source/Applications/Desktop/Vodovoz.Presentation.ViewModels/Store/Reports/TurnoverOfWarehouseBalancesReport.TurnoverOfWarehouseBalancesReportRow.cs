using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class TurnoverOfWarehouseBalancesReport
	{
		public class TurnoverOfWarehouseBalancesReportRow
		{
			public string WarehouseName { get; set; }
			public string NomanclatureName { get; set; }
			public IList<string> SliceValues { get; set; }
			public string Total { get; set; }
		}
	}
}
