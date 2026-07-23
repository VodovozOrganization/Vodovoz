using ClosedXML.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public partial class MarketingReport
		{
			private const string _templatePath = @".\Reports\Sales\MarketingReport.xlsx";
			private readonly List<DisplayRow> _displayRows = new List<DisplayRow>();

			private MarketingReport(
				DateTime startDate,
				DateTime endDate,
				IList<MarketingReportGroupKey> groups,
				IList<MarketingReportRawRow> periodRows,
				IList<MarketingReportRawRow> priorRows,
				IList<MarketingReportRawRow> historyRows,
				int totalCounterparties)
			{
				StartDate = startDate;
				EndDate = endDate;
				CreatedAt = DateTime.Now;

				var metricsByGroup = new List<MarketingReportMetrics>();

				foreach(var group in groups)
				{
					var groupPeriodRows = FilterByGroup(periodRows, group);
					var groupPriorRows = FilterByGroup(priorRows, group);
					var groupHistoryRows = FilterByGroup(historyRows, group);

					var metrics = CalculateMetrics(totalCounterparties, groupPeriodRows, groupPriorRows, groupHistoryRows, startDate, endDate);

					metricsByGroup.Add(metrics);
				}

				_displayRows.Add(new HeaderRow
				{
					Title = "Показатель",
					ColumnTitles = groups.Select(g => g.ColumnTitle).ToList()
				});

				AddMetricRow("Всего контрагентов", metricsByGroup.Select(m => (decimal?)m.TotalCounterparties).ToList());
				AddMetricRow("Процент активной базы", metricsByGroup.Select(m => (decimal?)m.ActiveBaseSharePercent).ToList(), MetricRow.PercentFormat);

				AddMetricRow("1. Average DAU", metricsByGroup.Select(m => (decimal?)m.AverageDau).ToList());
				AddMetricRow("2. Average WAU", metricsByGroup.Select(m => (decimal?)m.AverageWau).ToList());
				AddMetricRow("3. Average MAU", metricsByGroup.Select(m => (decimal?)m.AverageMau).ToList());
				AddMetricRow("4. Коэффициент липучести", metricsByGroup.Select(m => (decimal?)m.StickyFactorPercent).ToList(), MetricRow.PercentFormat);
				AddMetricRow("5. Частота заказов (день)", metricsByGroup.Select(m => (decimal?)m.OrdersFrequencyPerDay).ToList());
				AddMetricRow("5. Частота заказов (неделя)", metricsByGroup.Select(m => (decimal?)m.OrdersFrequencyPerWeek).ToList());
				AddMetricRow("5. Частота заказов (месяц)", metricsByGroup.Select(m => (decimal?)m.OrdersFrequencyPerMonth).ToList());
				AddMetricRow("6. Средний объём заказа (бутыли 19л)", metricsByGroup.Select(m => m.AverageBottlesPerOrder).ToList());
				AddMetricRow("7. Средний чек", metricsByGroup.Select(m => (decimal?)m.AverageCheck).ToList());
				AddMetricRow("8. Средний интервал между заказами (дней)", metricsByGroup.Select(m => m.AverageIntervalBetweenOrdersDays).ToList());
				AddMetricRow("9. Конверсия из пробного заказа в регулярный", metricsByGroup.Select(m => m.ConversionTrialToRegular_PeriodFirstOrder).ToList(), MetricRow.PercentFormat);
				AddMetricRow("10. Доля клиентов с доп.услугами", metricsByGroup.Select(m => (decimal?)m.AdditionalServicesSharePercent).ToList(), MetricRow.PercentFormat);
				AddMetricRow("11. Срок жизни клиента (дней)", metricsByGroup.Select(m => m.AverageCustomerLifetimeDays).ToList());
				AddMetricRow("12. Средняя оценка заказа", metricsByGroup.Select(m => (decimal?)m.AverageRating).ToList());
				AddMetricRow("13. Отток клиенток", metricsByGroup.Select(m => m.ChurnRatePercent).ToList(), MetricRow.PercentFormat);
				AddMetricRow("14. Retention", metricsByGroup.Select(m => m.RetentionRatePercent).ToList(), MetricRow.PercentFormat);

				AddMetricRow("15. LTV", metricsByGroup.Select(m => m.Ltv).ToList());
			}

			private void AddMetricRow(string title, IList<decimal?> values, string format = null)
			{
				var row = new MetricRow
				{
					Title = title,
					ValuesByGroup = values
				};

				if(format != null)
				{
					row.Format = format;
				}

				_displayRows.Add(row);
			}

			public string Title => "Маркетинговый отчет";

			public DateTime StartDate { get; }

			public DateTime EndDate { get; }

			public DateTime CreatedAt { get; }

			public List<DisplayRow> DisplayRows => _displayRows;

			public void Export(string path)
			{
				var template = new XLTemplate(_templatePath);
				template.AddVariable(this);
				template.Generate();
				template.SaveAs(path);
			}

			public static MarketingReport Create(
				DateTime start,
				DateTime end,
				bool splitByAbc,
				bool splitByOrderAuthor,
				IList<CounterpartyCompositeClassification> includedAbc,
				IList<OrderAuthorSubtype> includedAuthorSubtypes,
				Func<DateTime, DateTime, int[], OrderStatus[], IList<MarketingReportRawRow>> retrieveDataFunc,
				OrderStatus[] includedStatuses,
				Func<int> getTotalCounterpartiesFunc)
			{
				if(retrieveDataFunc is null)
				{
					throw new ArgumentNullException(nameof(retrieveDataFunc));
				}

				if(getTotalCounterpartiesFunc is null)
				{
					throw new ArgumentNullException(nameof(getTotalCounterpartiesFunc));
				}

				var periodRows = retrieveDataFunc(start, end, null, includedStatuses);

				var priorWindowStart = start.AddMonths(-3);
				var priorWindowEnd = start.AddDays(-1);
				var priorRows = retrieveDataFunc(priorWindowStart, priorWindowEnd, null, includedStatuses);

				var relevantClientIds = periodRows.Select(r => r.ClientId)
					.Union(priorRows.Select(r => r.ClientId))
					.Distinct()
					.ToArray();

				
				var historyWindowStart = new DateTime(2010, 1, 1); // Заменить

				var historyRows = relevantClientIds.Length > 0
					? retrieveDataFunc(historyWindowStart, end, relevantClientIds, includedStatuses)
					: new List<MarketingReportRawRow>();

				var totalCounterparties = getTotalCounterpartiesFunc();

				var groups = BuildGroups(splitByAbc, splitByOrderAuthor, includedAbc, includedAuthorSubtypes, periodRows);

				return new MarketingReport(start, end, groups, periodRows, priorRows, historyRows, totalCounterparties);
			}
		}
	}
}
