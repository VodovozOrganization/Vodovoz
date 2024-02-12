using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartyClassificationCalculationSettingsMap : ClassMap<CounterpartyClassificationCalculationSettings>
	{
		public CounterpartyClassificationCalculationSettingsMap()
		{
			Table("counterparty_classification_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PeriodInMonths).Column("period_in_months");
			Map(x => x.BottlesCountAClassificationFrom).Column("bottles_count_a_classification_from");
			Map(x => x.BottlesCountCClassificationTo).Column("bottles_count_c_classification_to");
			Map(x => x.OrdersCountXClassificationFrom).Column("orders_count_x_classification_from");
			Map(x => x.OrdersCountZClassificationTo).Column("orders_count_z_classification_to");
			Map(x => x.SettingsCreationDate).Column("settings_creation_date");
		}
	}
}
