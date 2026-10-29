using System.Collections.Generic;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportMetrics
	{
		public int TotalCounterparties { get; set; }
		public int ActiveClientsCount { get; set; }
		public decimal ActiveBasePercent { get; set; }
		public Dictionary<string, int> DailyActiveClients { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, int> WeeklyActiveClients { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, int> MonthlyActiveClients { get; set; } = new Dictionary<string, int>();
		public double AverageDau { get; set; }
		public double AverageWau { get; set; }
		public double AverageMau { get; set; }
		public double StickyFactor { get; set; }
		public decimal OrdersFrequencyPerDay { get; set; }
		public decimal OrdersFrequencyPerWeek { get; set; }
		public decimal OrdersFrequencyPerMonth { get; set; }
		public decimal AverageOrderVolume19L { get; set; }
		public decimal AverageCheck { get; set; }
		public double AverageIntervalBetweenOrdersDays { get; set; }
		public decimal TrialToRegularConversion { get; set; }
		public decimal AdditionalServicesClientsShare { get; set; }
		public double CustomerLifetimeDays { get; set; }
		public double CustomerLifetimeMonths { get; set; }
		public double AverageSatisfaction { get; set; }
		public decimal ChurnRate { get; set; }
		public decimal RetentionRate { get; set; }
		public decimal LifetimeValue { get; set; }
	}
}
