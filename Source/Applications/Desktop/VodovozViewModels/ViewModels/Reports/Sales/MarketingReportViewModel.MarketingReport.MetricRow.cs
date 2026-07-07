using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public partial class MarketingReport
		{
			public class MetricRow : DisplayRow
			{
				private const string _numberFormat = "0.##";
				private const string _percentFormat = "0.##'%'";

				private List<string> _dynamicColumns;
				public IList<decimal?> ValuesByGroup { get; set; } = new List<decimal?>();
				public string Format { get; set; } = _numberFormat;

				public override IList<string> DynamicColumns
				{
					get
					{
						if(_dynamicColumns == null)
						{
							_dynamicColumns = ValuesByGroup.Select(v => v.HasValue ? v.Value.ToString(Format) : "-").ToList();
						}
						return _dynamicColumns;
					}
					set => _dynamicColumns = new List<string>(value);
				}

				public static string PercentFormat => _percentFormat;
			}
		}
	}
}
