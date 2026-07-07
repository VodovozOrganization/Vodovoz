using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public enum MarketingReportGroupDimension
		{
			All,
			Abc,
			Site,
			MobileApp,
			Subdivision
		}

		public class MarketingReportGroupKey
		{
			public string ColumnTitle { get; set; }
			public MarketingReportGroupDimension Dimension { get; set; }
			public CounterpartyCompositeClassification? AbcValue { get; set; }
			public int? SubdivisionId { get; set; }
		}
	}
}
