
namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public class MarketingReportMetrics
		{
			public int TotalCounterparties { get; set; }
			public decimal ActiveBaseSharePercent { get; set; }

			public decimal AverageDau { get; set; }
			public decimal AverageWau { get; set; }
			public decimal AverageMau { get; set; }

			public decimal StickyFactorPercent { get; set; }

			public decimal OrdersFrequencyPerDay { get; set; }
			public decimal OrdersFrequencyPerWeek { get; set; }
			public decimal OrdersFrequencyPerMonth { get; set; }

			public decimal? AverageBottlesPerOrder { get; set; }
			public decimal AverageCheck { get; set; }
			public decimal? AverageIntervalBetweenOrdersDays { get; set; }

			public decimal? ConversionTrialToRegular_PeriodFirstOrder { get; set; }
			public decimal? AdditionalServicesSharePercent { get; set; }
			public decimal? AverageCustomerLifetimeDays { get; set; }
			public decimal AverageRating { get; set; }
			public decimal? ChurnRatePercent { get; set; }
			public decimal? RetentionRatePercent { get; set; }
			public decimal? Ltv { get; set; }
		}
	}
}
