using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel
	{
		public static class ClassificationCalculationReport
		{
			public static void GenerateReport(
				IDictionary<int, CounterpartyClassification> newClassifications,
				IDictionary<int, CounterpartyClassification> oldClassifications,
				IDictionary<int, string> counterpartyNames)
			{
				var rows = CreateRows(newClassifications, oldClassifications, counterpartyNames);

				Export(rows);
			}

			private static IEnumerable<ClassificationCalculationReportRow> CreateRows(
				IDictionary<int, CounterpartyClassification> newClassifications,
				IDictionary<int, CounterpartyClassification> oldClassifications,
				IDictionary<int, string> counterpartyNames)
			{
				var rows = new List<ClassificationCalculationReportRow>();

				foreach(var classification in newClassifications.Values)
				{
					var hasOldClassification =
						oldClassifications.TryGetValue(classification.CounterpartyId, out CounterpartyClassification oldClassification);

					if(hasOldClassification
						&& classification.ClassificationByBottlesCount == oldClassification.ClassificationByBottlesCount
						&& classification.ClassificationByOrdersCount == oldClassification.ClassificationByOrdersCount)
					{
						continue;
					}

					var row = new ClassificationCalculationReportRow();

					row.CounterpartyId = classification.CounterpartyId;

					row.CounterpartyName =
						counterpartyNames.TryGetValue(classification.CounterpartyId, out string name)
						? name
						: $"Имя не указано. Id = {classification.CounterpartyId}";

					row.NewAverageBottlesCount = classification.BottlesPerMonthAverageCount;
					row.NewAverageOrdersCount = classification.OrdersPerMonthAverageCount;
					row.NewAverageMoneyTurnoverSum = classification.MoneyTurnoverPerMonthAverageSum;
					row.NewClassificationByBottles = classification.ClassificationByBottlesCount;
					row.NewClassificationByOrders = classification.ClassificationByOrdersCount;

					if(hasOldClassification)
					{
						row.OldAverageBottlesCount = oldClassification.BottlesPerMonthAverageCount;
						row.OldAverageOrdersCount = oldClassification.OrdersPerMonthAverageCount;
						row.OldAverageMoneyTurnoverSum = oldClassification.MoneyTurnoverPerMonthAverageSum;
						row.OldClassificationByBottles = oldClassification.ClassificationByBottlesCount;
						row.OldClassificationByOrders = oldClassification.ClassificationByOrdersCount;
					}

					rows.Add(row);
				}

				return rows;
			}

			private static void Export(IEnumerable<ClassificationCalculationReportRow> rows)
			{

				var groupedByBottlesClassification = (from r in rows
													  group r by new { r.NewClassificationByBottles, r.OldClassificationByBottles })
													 .ToDictionary(g => g.Key, g => g.ToList());

				var groupedByOrdersClassification = (from r in rows
													 group r by new { r.NewClassificationByOrders, r.OldClassificationByOrders })
													 .ToDictionary(g => g.Key, g => g.ToList());
			}
		}
	}
}
