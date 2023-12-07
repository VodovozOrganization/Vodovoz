using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel
	{
		public class ClassificationCalculationReportRow
		{
			public int CounterpartyId { get; set; }
			public string CounterpartyName { get; set; }

			public decimal NewAverageBottlesCount { get; set; }
			public decimal NewAverageOrdersCount { get; set; }
			public decimal NewAverageMoneyTurnoverSum { get; set; }
			public CounterpartyClassificationByBottlesCount NewClassificationByBottles { get; set; }
			public CounterpartyClassificationByOrdersCount NewClassificationByOrders { get; set; }

			public decimal? OldAverageBottlesCount { get; set; }
			public decimal? OldAverageOrdersCount { get; set; }
			public decimal? OldAverageMoneyTurnoverSum { get; set; }
			public CounterpartyClassificationByBottlesCount? OldClassificationByBottles { get; set; }
			public CounterpartyClassificationByOrdersCount? OldClassificationByOrders { get; set; }
		}
	}
}
