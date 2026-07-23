using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public partial class MarketingReport
		{
			public class HeaderRow : DisplayRow
			{
				public IEnumerable<string> ColumnTitles { get; set; }
				private List<string> _dynamicColumns;
				public override IList<string> DynamicColumns
				{
					get
					{
						if (_dynamicColumns == null)
						{
							_dynamicColumns = ColumnTitles.ToList();
						}
						return _dynamicColumns;
					}
					set => _dynamicColumns = new List<string>(value);
				}
			}
		}
	}
}
