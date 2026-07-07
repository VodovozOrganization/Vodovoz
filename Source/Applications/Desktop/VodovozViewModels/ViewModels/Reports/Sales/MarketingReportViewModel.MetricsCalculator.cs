using System;
using System.Collections.Generic;
using System.Linq;


namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		private static MarketingReportMetrics CalculateMetrics(
			int totalCounterparties,
			IList<MarketingReportRawRow> periodRows,
			IList<MarketingReportRawRow> priorRows,
			IList<MarketingReportRawRow> historyRows,
			DateTime start,
			DateTime end)

		{
			var metrics = new MarketingReportMetrics();
			var periodClientIds = new HashSet<int>();

			foreach(var row in periodRows)
			{
				periodClientIds.Add(row.ClientId);
			}

			metrics.TotalCounterparties = totalCounterparties;
			metrics.ActiveBaseSharePercent = totalCounterparties > 0 ? (decimal)periodClientIds.Count / totalCounterparties * 100m : 0;

			metrics.AverageDau = CalculateAverageActiveUsers(periodRows, start, end, 1);
			metrics.AverageWau = CalculateAverageActiveUsers(periodRows, start, end, 7);
			metrics.AverageMau = CalculateAverageMau(periodRows, start, end);

			metrics.StickyFactorPercent = metrics.AverageMau > 0
							   ? metrics.AverageDau / metrics.AverageMau * 100m
							   : 0m;

			var totalDays = (end.Date - start.Date).Days + 1;
			var totalWeeks = totalDays / 7m;
			var totalMonths = CountMonthsInPeriod(start, end);

			var ordersCount = periodRows.Count;
			var clientsCount = periodClientIds.Count;

			metrics.OrdersFrequencyPerDay = clientsCount > 0 && totalDays > 0
							   ? (decimal)ordersCount / clientsCount / totalDays
							   : 0m;

			metrics.OrdersFrequencyPerWeek = clientsCount > 0 && totalWeeks > 0
					? (decimal)ordersCount / clientsCount / totalWeeks
					: 0m;

			metrics.OrdersFrequencyPerMonth = clientsCount > 0 && totalMonths > 0
					? (decimal)ordersCount / clientsCount / totalMonths
					: 0m;

			metrics.AverageBottlesPerOrder = ordersCount > 0
					? periodRows.Sum(r => r.BottlesCount19L) / ordersCount
					: (decimal?)null;

			var totalOrderSum = periodRows.Sum(r => r.OrderSum);
			metrics.AverageCheck = ordersCount > 0 ? totalOrderSum / ordersCount : 0m;

			metrics.AverageIntervalBetweenOrdersDays = CalculateAverageIntervalBetweenOrders(periodRows);

			metrics.ConversionTrialToRegular_PeriodFirstOrder = CalculateConversionPeriodBased(periodRows);

			var clientsWithAdditionalService = periodRows
								.Where(r => r.HasAdditionalServiceFlag == 1)
								.Select(r => r.ClientId)
								.Distinct()
								.Count();

			metrics.AdditionalServicesSharePercent = clientsCount > 0
					? (decimal)clientsWithAdditionalService / clientsCount * 100m
					: 0m;

			metrics.AverageCustomerLifetimeDays = CalculateAverageCustomerLifetime(periodClientIds, historyRows);

			var ratedOrders = periodRows.Select(r => r.Rating ?? 5).ToList();
			metrics.AverageRating = ratedOrders.Count > 0 ? (decimal)ratedOrders.Average() : 0m;

			var priorClientIds = new HashSet<int>();

			foreach(var row in priorRows)
			{
				priorClientIds.Add(row.ClientId);
			}

			if(priorClientIds.Count > 0)
			{
				var churned = 0;

				foreach(var clientId in priorClientIds)
				{
					if(!periodClientIds.Contains(clientId))
					{
						churned++;
					}
				}

				metrics.ChurnRatePercent = (decimal)churned / priorClientIds.Count * 100m;

				var newClientsCount = periodRows
						.Where(r => r.IsFirstOrderEver)
						.Select(r => r.ClientId)
						.Distinct()
						.Count();
				metrics.RetentionRatePercent = (decimal)(clientsCount - newClientsCount) / priorClientIds.Count * 100m;
			}
			else
			{
				metrics.ChurnRatePercent = null;
				metrics.RetentionRatePercent = null;
			}
			if(metrics.AverageCustomerLifetimeDays.HasValue)
			{
				var lifetimeMonths = metrics.AverageCustomerLifetimeDays.Value / 30m;
				metrics.Ltv = metrics.AverageCheck * metrics.OrdersFrequencyPerMonth * lifetimeMonths;
			}
			else
			{
				metrics.Ltv = null;
			}

			return metrics;
		}
		private static int CountMonthsInPeriod(DateTime start, DateTime end)
		{
			var totalMonths = 0;
			var cursor = new DateTime(start.Year, start.Month, 1);
			var endMonth = new DateTime(end.Year, end.Month, 1);

			while(cursor <= endMonth)
			{
				totalMonths++;
				cursor = cursor.AddMonths(1);
			}

			return totalMonths;
		}
		private static decimal CalculateAverageActiveUsers(IList<MarketingReportRawRow> rows, DateTime start, DateTime end, int bucketDays)
		{
			var totalDays = (end.Date - start.Date).Days + 1;

			if(totalDays <= 0)
			{
				return 0m;
			}

			var bucketsCount = (int)Math.Ceiling(totalDays / (double)bucketDays);
			var bucketClients = new List<HashSet<int>>();

			for(var i = 0; i < bucketsCount; i++)
			{
				bucketClients.Add(new HashSet<int>());
			}

			foreach(var row in rows)
			{
				var dayOffset = (row.OrderDate.Date - start.Date).Days;

				if(dayOffset < 0 || dayOffset >= totalDays)
				{
					continue;
				}

				var bucketIndex = dayOffset / bucketDays;
				bucketClients[bucketIndex].Add(row.ClientId);
			}

			var totalActiveUsers = 0;

			foreach(var bucket in bucketClients)
			{
				totalActiveUsers += bucket.Count;
			}

			return (decimal)totalActiveUsers / bucketsCount;
		}
		private static decimal CalculateAverageMau(IList<MarketingReportRawRow> rows, DateTime start, DateTime end)
		{
			var months = new List<Tuple<int, int>>();
			var cursor = new DateTime(start.Year, start.Month, 1);
			var endMonth = new DateTime(end.Year, end.Month, 1);

			while(cursor <= endMonth)
			{
				months.Add(new Tuple<int, int>(cursor.Year, cursor.Month));
				cursor = cursor.AddMonths(1);
			}

			if(months.Count == 0)
			{
				return 0m;
			}

			var byMonth = new Dictionary<Tuple<int, int>, HashSet<int>>();
			foreach(var month in months)
			{
				byMonth[month] = new HashSet<int>();
			}

			foreach(var row in rows)
			{
				var key = new Tuple<int, int>(row.OrderDate.Year, row.OrderDate.Month);
				HashSet<int> clientsInMonth;

				if(byMonth.TryGetValue(key, out clientsInMonth))
				{
					clientsInMonth.Add(row.ClientId);
				}
			}
			var total = 0;

			foreach(var month in months)
			{
				total += byMonth[month].Count;
			}

			return (decimal)total / months.Count;
		}
		private static decimal? CalculateAverageIntervalBetweenOrders(IList<MarketingReportRawRow> periodRows)
		{
			var intervalsInDays = new List<double>();
			var groupedByClient = periodRows.GroupBy(r => r.ClientId);

			foreach(var clientGroup in groupedByClient)
			{
				var dates = clientGroup.Select(r => r.OrderDate.Date).Distinct().OrderBy(d => d).ToList();

				for(var i = 1; i < dates.Count; i++)
				{
					intervalsInDays.Add((dates[i] - dates[i - 1]).TotalDays);
				}
			}

			return intervalsInDays.Count > 0 ? (decimal?)intervalsInDays.Average() : null;
		}
		private static decimal? CalculateConversionPeriodBased(IList<MarketingReportRawRow> periodRows)
		{
			var groupedByClient = periodRows.GroupBy(r => r.ClientId).ToList();

			if(groupedByClient.Count == 0)
			{
				return null;
			}

			var clientsWithSecondOrder = groupedByClient.Count(g => g.Select(r => r.OrderId).Distinct().Count() >= 2);

			return (decimal)clientsWithSecondOrder / groupedByClient.Count * 100m;
		}
		private static decimal? CalculateAverageCustomerLifetime(
					   HashSet<int> periodClientIds,
					   IList<MarketingReportRawRow> historyRows)
		{
			if(periodClientIds.Count == 0)
			{
				return null;
			}

			var lifetimesInDays = new List<double>();

			var groupedByClient = historyRows
					.Where(r => periodClientIds.Contains(r.ClientId))
					.GroupBy(r => r.ClientId);

			foreach(var clientGroup in groupedByClient)
			{
				var minDate = clientGroup.Min(r => r.OrderDate.Date);
				var maxDate = clientGroup.Max(r => r.OrderDate.Date);
				lifetimesInDays.Add((maxDate - minDate).TotalDays);
			}

			return lifetimesInDays.Count > 0 ? (decimal?)lifetimesInDays.Average() : null;
		}
	}


}

