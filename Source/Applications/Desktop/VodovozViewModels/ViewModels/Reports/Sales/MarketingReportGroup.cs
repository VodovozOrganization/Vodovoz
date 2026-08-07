namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportGroup
	{
		public MarketingReportGroup(string title, MarketingReportMetrics metrics)
		{
			Title = title;
			Metrics = metrics;
		}

		public string Title { get; }
		public MarketingReportMetrics Metrics { get; }
	}
}
