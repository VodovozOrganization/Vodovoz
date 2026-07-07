using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DateTimeHelpers;
using Gamma.Utilities;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class MarketingReport
	{
		private const int DefaultRatingWhenMissing = 5;

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string FiltersDescription { get; }
		public MarketingReportGroupingType GroupingType { get; }
		public MarketingReportDateType DateType { get; }
		public DateTime CreatedAt { get; }
		public int TotalCounterparties { get; }
		public IReadOnlyList<MarketingReportGroup> Groups { get; }
		public IReadOnlyList<MarketingReportDisplayRow> DisplayRows { get; }

		private MarketingReport(
			DateTime startDate,
			DateTime endDate,
			string filtersDescription,
			MarketingReportGroupingType groupingType,
			MarketingReportDateType dateType,
			int totalCounterparties,
			IReadOnlyList<MarketingReportGroup> groups)
		{
			StartDate = startDate;
			EndDate = endDate;
			FiltersDescription = filtersDescription;
			GroupingType = groupingType;
			DateType = dateType;
			TotalCounterparties = totalCounterparties;
			Groups = groups;
			CreatedAt = DateTime.Now;
			DisplayRows = BuildDisplayRows(groups);
		}

		public static MarketingReport Create(
			DateTime startDate,
			DateTime endDate,
			string filtersDescription,
			MarketingReportGroupingType groupingType,
			MarketingReportDateType dateType,
			int totalCounterparties,
			IList<MarketingReportOrderNode> orders,
			IList<MarketingReportClientHistoryNode> clientHistories)
		{
			var groups = BuildGroups(
				startDate,
				endDate,
				groupingType,
				dateType,
				totalCounterparties,
				orders,
				clientHistories);

			return new MarketingReport(
				startDate,
				endDate,
				filtersDescription,
				groupingType,
				dateType,
				totalCounterparties,
				groups);
		}

		public void Export(string path) => new ExcelExporter(this).Export(path);

		private static IReadOnlyList<MarketingReportGroup> BuildGroups(
			DateTime startDate,
			DateTime endDate,
			MarketingReportGroupingType groupingType,
			MarketingReportDateType dateType,
			int totalCounterparties,
			IList<MarketingReportOrderNode> orders,
			IList<MarketingReportClientHistoryNode> clientHistories)
		{
			var historyByClient = clientHistories.ToDictionary(x => x.ClientId);

			switch(groupingType)
			{
				case MarketingReportGroupingType.AbcCategory:
					return orders
						.GroupBy(x => x.AbcClassification ?? CounterpartyCompositeClassification.New)
						.OrderBy(x => x.Key)
						.Select(group => CreateGroup(
							group.Key.GetEnumTitle(),
							group.ToList(),
							historyByClient,
							startDate,
							endDate,
							dateType,
							totalCounterparties))
						.ToList();
				case MarketingReportGroupingType.OrderAuthor:
					return orders
						.GroupBy(x => new { x.AuthorId, x.AuthorName })
						.OrderBy(x => x.Key.AuthorName)
						.Select(group => CreateGroup(
							group.Key.AuthorName,
							group.ToList(),
							historyByClient,
							startDate,
							endDate,
							dateType,
							totalCounterparties))
						.ToList();
				default:
					return new[]
					{
						CreateGroup(
							"Все",
							orders,
							historyByClient,
							startDate,
							endDate,
							dateType,
							totalCounterparties)
					};
			}
		}

		private static MarketingReportGroup CreateGroup(
			string title,
			IList<MarketingReportOrderNode> orders,
			IDictionary<int, MarketingReportClientHistoryNode> historyByClient,
			DateTime startDate,
			DateTime endDate,
			MarketingReportDateType dateType,
			int totalCounterparties)
		{
			var metrics = CalculateMetrics(orders, historyByClient, startDate, endDate, dateType, totalCounterparties);
			return new MarketingReportGroup(title, metrics);
		}

		private static MarketingReportMetrics CalculateMetrics(
			IList<MarketingReportOrderNode> orders,
			IDictionary<int, MarketingReportClientHistoryNode> historyByClient,
			DateTime startDate,
			DateTime endDate,
			MarketingReportDateType dateType,
			int totalCounterparties)
		{
			var periodOrders = orders
				.Where(x => IsInPeriod(x.GetReportDate(dateType), startDate, endDate))
				.ToList();

			var activeClientIds = new HashSet<int>(periodOrders.Select(x => x.ClientId));
			var activeClientsCount = activeClientIds.Count;

			var dailyActive = GroupDistinctClientsByPeriod(periodOrders, dateType, DateTimeSliceType.Day, startDate, endDate);
			var weeklyActive = GroupDistinctClientsByPeriod(periodOrders, dateType, DateTimeSliceType.Week, startDate, endDate);
			var monthlyActive = GroupDistinctClientsByPeriod(periodOrders, dateType, DateTimeSliceType.Month, startDate, endDate);

			var averageDau = dailyActive.Values.Any() ? dailyActive.Values.Average() : 0;
			var averageWau = weeklyActive.Values.Any() ? weeklyActive.Values.Average() : 0;
			var averageMau = monthlyActive.Values.Any() ? monthlyActive.Values.Average() : 0;

			var ordersCount = periodOrders.Count;
			var orderSums = periodOrders.Sum(x => x.OrderSum);
			var bottlesCount = periodOrders.Sum(x => x.Bottles19LCount);

			var periodDays = Math.Max(1, (endDate.Date - startDate.Date).Days + 1);
			var periodWeeks = Math.Max(1d, periodDays / 7d);
			var periodMonths = Math.Max(1d, GetMonthsCount(startDate, endDate));

			var clientsWithAdditionalServices = periodOrders
				.Where(x => x.HasAdditionalServices)
				.Select(x => x.ClientId)
				.Distinct()
				.Count();

			var ratings = periodOrders
				.Select(x => x.Rating ?? DefaultRatingWhenMissing)
				.ToList();

			var activeAtStart = GetActiveClientsAtDate(historyByClient, startDate.AddDays(-1), dateType);
			var activeAtEnd = activeClientIds;
			var newClients = new HashSet<int>(activeAtEnd
				.Where(clientId => historyByClient.TryGetValue(clientId, out var history)
					&& IsInPeriod(GetHistoryDate(history, dateType, true), startDate, endDate)
					&& history.OrdersCount == 1));

			var churnedClients = activeAtStart.Where(clientId => !activeAtEnd.Contains(clientId)).Count();
			var retentionBase = activeAtStart.Count;
			var retainedClients = activeAtEnd.Count(clientId => activeAtStart.Contains(clientId));
			var retentionRate = retentionBase > 0 ? (decimal)(retainedClients - newClients.Count) / retentionBase : 0;

			var customerLifetimeDays = activeClientIds
				.Select(clientId =>
				{
					if(!historyByClient.TryGetValue(clientId, out var history))
					{
						return 0d;
					}

					return (GetHistoryDate(history, dateType, false) - GetHistoryDate(history, dateType, true)).TotalDays;
				})
				.Where(days => days > 0)
				.ToList();

			var averageLifetimeDays = customerLifetimeDays.Any() ? customerLifetimeDays.Average() : 0;
			var averageLifetimeMonths = averageLifetimeDays / 30d;
			var averageCheck = ordersCount > 0 ? orderSums / ordersCount : 0;
			var monthlyFrequency = activeClientsCount > 0 ? ordersCount / periodMonths / activeClientsCount : 0;
			var ltv = (decimal)averageCheck * (decimal)monthlyFrequency * (decimal)averageLifetimeMonths;

			var trialClients = activeClientIds
				.Where(clientId => historyByClient.TryGetValue(clientId, out var history)
					&& IsInPeriod(GetHistoryDate(history, dateType, true), startDate, endDate))
				.ToList();

			var convertedClients = trialClients
				.Count(clientId => historyByClient.TryGetValue(clientId, out var history) && history.OrdersCount >= 2);

			var averageIntervals = CalculateAverageOrderIntervals(periodOrders, dateType);

			return new MarketingReportMetrics
			{
				TotalCounterparties = totalCounterparties,
				ActiveClientsCount = activeClientsCount,
				ActiveBasePercent = totalCounterparties > 0 ? (decimal)activeClientsCount / totalCounterparties : 0,
				DailyActiveClients = dailyActive,
				WeeklyActiveClients = weeklyActive,
				MonthlyActiveClients = monthlyActive,
				AverageDau = averageDau,
				AverageWau = averageWau,
				AverageMau = averageMau,
				StickyFactor = averageMau > 0 ? averageDau / averageMau : 0,
				OrdersFrequencyPerDay = activeClientsCount > 0 ? ordersCount / (decimal)periodDays / activeClientsCount : 0,
				OrdersFrequencyPerWeek = activeClientsCount > 0 ? ordersCount / (decimal)periodWeeks / activeClientsCount : 0,
				OrdersFrequencyPerMonth = activeClientsCount > 0 ? ordersCount / (decimal)periodMonths / activeClientsCount : 0,
				AverageOrderVolume19L = ordersCount > 0 ? bottlesCount / ordersCount : 0,
				AverageCheck = averageCheck,
				AverageIntervalBetweenOrdersDays = averageIntervals,
				TrialToRegularConversion = trialClients.Count > 0 ? (decimal)convertedClients / trialClients.Count : 0,
				AdditionalServicesClientsShare = activeClientsCount > 0 ? (decimal)clientsWithAdditionalServices / activeClientsCount : 0,
				CustomerLifetimeDays = averageLifetimeDays,
				CustomerLifetimeMonths = averageLifetimeMonths,
				AverageSatisfaction = ratings.Any() ? ratings.Average() : 0,
				ChurnRate = retentionBase > 0 ? (decimal)churnedClients / retentionBase : 0,
				RetentionRate = retentionRate,
				LifetimeValue = ltv
			};
		}

		private static double CalculateAverageOrderIntervals(
			IList<MarketingReportOrderNode> orders,
			MarketingReportDateType dateType)
		{
			var intervals = orders
				.GroupBy(x => x.ClientId)
				.Select(group => group
					.Select(x => x.GetReportDate(dateType))
					.Where(date => date != DateTime.MinValue)
					.OrderBy(date => date)
					.ToList())
				.Where(dates => dates.Count > 1)
				.SelectMany(dates =>
				{
					var clientIntervals = new List<double>();
					for(var i = 1; i < dates.Count; i++)
					{
						clientIntervals.Add((dates[i] - dates[i - 1]).TotalDays);
					}

					return clientIntervals;
				})
				.ToList();

			return intervals.Any() ? intervals.Average() : 0;
		}

		private static HashSet<int> GetActiveClientsAtDate(
			IDictionary<int, MarketingReportClientHistoryNode> historyByClient,
			DateTime date,
			MarketingReportDateType dateType)
		{
			return new HashSet<int>(historyByClient
				.Where(pair => GetHistoryDate(pair.Value, dateType, true) <= date
					&& GetHistoryDate(pair.Value, dateType, false) >= date.AddMonths(-3))
				.Select(pair => pair.Key));
		}

		private static DateTime GetHistoryDate(
			MarketingReportClientHistoryNode history,
			MarketingReportDateType dateType,
			bool first)
		{
			return first ? history.FirstOrderDate : history.LastOrderDate;
		}

		private static Dictionary<string, int> GroupDistinctClientsByPeriod(
			IList<MarketingReportOrderNode> orders,
			MarketingReportDateType dateType,
			DateTimeSliceType sliceType,
			DateTime startDate,
			DateTime endDate)
		{
			var slices = DateTimeSliceFactory.CreateSlices(sliceType, startDate, endDate).ToList();
			var result = slices.ToDictionary(slice => slice.ToString(), _ => 0);

			foreach(var slice in slices)
			{
				var clientsCount = orders
					.Where(order =>
					{
						var orderDate = order.GetReportDate(dateType);
						return orderDate >= slice.StartDate && orderDate <= slice.EndDate;
					})
					.Select(order => order.ClientId)
					.Distinct()
					.Count();

				result[slice.ToString()] = clientsCount;
			}

			return result;
		}

		private static int GetMonthsCount(DateTime startDate, DateTime endDate)
		{
			var months = 0;
			for(var monthDate = startDate.Date; monthDate <= endDate.Date; monthDate = monthDate.AddMonths(1))
			{
				months++;
			}

			return Math.Max(months, 1);
		}

		private static bool IsInPeriod(DateTime date, DateTime startDate, DateTime endDate) =>
			date.Date >= startDate.Date && date.Date <= endDate.Date;

		private static IReadOnlyList<MarketingReportDisplayRow> BuildDisplayRows(IReadOnlyList<MarketingReportGroup> groups)
		{
			var rows = new List<MarketingReportDisplayRow>();

			foreach(var group in groups)
			{
				if(groups.Count > 1)
				{
					rows.Add(MarketingReportDisplayRow.Section(group.Title));
				}

				rows.Add(MarketingReportDisplayRow.Metric("Всего контрагентов", group.Metrics.TotalCounterparties.ToString("N0", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric(
					"Процент активной базы",
					group.Metrics.ActiveClientsCount.ToString("N0", CultureInfo.CurrentCulture),
					group.Metrics.ActiveBasePercent.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Average DAU", group.Metrics.AverageDau.ToString("N0", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Average WAU", group.Metrics.AverageWau.ToString("N0", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Average MAU", group.Metrics.AverageMau.ToString("N0", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Sticky Factor", group.Metrics.StickyFactor.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Частота заказов (день)", group.Metrics.OrdersFrequencyPerDay.ToString("N2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Частота заказов (неделя)", group.Metrics.OrdersFrequencyPerWeek.ToString("N2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Частота заказов (месяц)", group.Metrics.OrdersFrequencyPerMonth.ToString("N2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Средний объём заказа (19 л)", group.Metrics.AverageOrderVolume19L.ToString("N1", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Средний чек", group.Metrics.AverageCheck.ToString("N0", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Средний интервал между заказами", $"{group.Metrics.AverageIntervalBetweenOrdersDays:N1} дней"));
				rows.Add(MarketingReportDisplayRow.Metric("Конверсия пробный → регулярный", group.Metrics.TrialToRegularConversion.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Доля клиентов с доп.услугами", group.Metrics.AdditionalServicesClientsShare.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Customer Lifetime", $"{group.Metrics.CustomerLifetimeDays:N0} дней ({group.Metrics.CustomerLifetimeMonths:N1} мес.)"));
				rows.Add(MarketingReportDisplayRow.Metric("Средняя удовлетворённость", group.Metrics.AverageSatisfaction.ToString("N2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Churn Rate", group.Metrics.ChurnRate.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("Retention Rate", group.Metrics.RetentionRate.ToString("P2", CultureInfo.CurrentCulture)));
				rows.Add(MarketingReportDisplayRow.Metric("LTV", group.Metrics.LifetimeValue.ToString("N0", CultureInfo.CurrentCulture)));
			}

			return rows;
		}
	}

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
