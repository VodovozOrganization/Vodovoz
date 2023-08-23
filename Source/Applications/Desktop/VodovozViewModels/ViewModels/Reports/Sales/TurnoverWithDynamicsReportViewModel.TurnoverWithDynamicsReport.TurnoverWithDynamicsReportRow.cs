using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			public class TurnoverWithDynamicsReportRow
			{
				public string Title { get; set; }

				public string Phones { get; set; } = string.Empty;
				public string Emails { get; set; } = string.Empty;

				public string Index { get; set; } = string.Empty;

				public RowTypes RowType { get; set; }

				public bool IsTotalsRow => RowType == RowTypes.Totals;

				public bool IsSubheaderRow => RowType == RowTypes.Subheader;

				public IList<decimal> SliceColumnValues { get; set; }

				public IList<string> DynamicColumns { get; set; }

				public decimal RowTotal => SliceColumnValues.Sum();

				public TurnoverWithDynamicsReportLastSaleDetails LastSaleDetails { get; set; }

				public enum RowTypes
				{
					Values,
					Totals,
					Subheader
				}
			}
		}
	}
}
