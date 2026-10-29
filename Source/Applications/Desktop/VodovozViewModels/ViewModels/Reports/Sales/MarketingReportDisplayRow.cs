namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportDisplayRow
	{
		public bool IsSection { get; private set; }
		public string Title { get; private set; }
		public string Value { get; private set; }
		public string AdditionalValue { get; private set; }

		public static MarketingReportDisplayRow Section(string title) =>
			new MarketingReportDisplayRow { IsSection = true, Title = title };

		public static MarketingReportDisplayRow Metric(string title, string value, string additionalValue = null) =>
			new MarketingReportDisplayRow { Title = title, Value = value, AdditionalValue = additionalValue };
	}
}
