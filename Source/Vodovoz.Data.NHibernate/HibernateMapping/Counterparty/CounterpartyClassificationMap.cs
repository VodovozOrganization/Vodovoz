using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client.CounterpartyClassification;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartyClassificationMap : ClassMap<CounterpartyClassification>
	{
		public CounterpartyClassificationMap()
		{
			Table("counterparty_classification");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.ClassificationByBottlesCount).Column("classification_by_bottles_count");
			Map(x => x.ClassificationByOrdersCount).Column("classification_by_orders_count");
			Map(x => x.BottlesPerMonthAverageCount).Column("bottles_per_month_average_count");
			Map(x => x.OrdersPerMonthAverageCount).Column("orders_per_month_average_count");
			Map(x => x.MoneyTurnoverPerMonthAverageSum).Column("money_turnover_per_month_average_sum");
			Map(x => x.ClassificationCalculationDate).Column("calculation_date");
		}
	}
}
