using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Logistic.Cars
{
	public class DriverScheduleItemMap : ClassMap<DriverScheduleItem>
	{
		public DriverScheduleItemMap()
		{
			Table("driver_schedule_items");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Date)
				.Column("date");
			Map(x => x.MorningAddresses)
				.Column("morning_addresses");
			Map(x => x.MorningBottles)
				.Column("morning_bottles");
			Map(x => x.EveningAddresses)
				.Column("evening_addresses");
			Map(x => x.EveningBottles)
				.Column("evening_bottles");

			References(x => x.DriverSchedule)
				.Column("driver_schedule_id");
			References(x => x.CarEventType)
				.Column("car_event_type_id");
		}
	}
}
