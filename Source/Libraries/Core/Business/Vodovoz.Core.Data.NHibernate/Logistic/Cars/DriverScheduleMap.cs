using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Logistic.Cars
{
	public class DriverScheduleMap : ClassMap<DriverScheduleEntity>
	{
		public DriverScheduleMap()
		{
			Table("driver_schedules");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.ArrivalTime)
				.Column("arrival_time")
				.Nullable();
			Map(x => x.MorningAddressesPotential)
				.Column("morning_addresses_potential");
			Map(x => x.MorningBottlesPotential)
				.Column("morning_bottles_potential");
			Map(x => x.EveningAddressesPotential)
				.Column("evening_addresses_potential");
			Map(x => x.EveningBottlesPotential)
				.Column("evening_bottles_potential");
			Map(x => x.LastChangeTime)
				.Column("last_change_time")
				.Nullable();
			Map(x => x.Comment)
				.Column("comment");

			References(x => x.Driver)
				.Column("driver_id");

			HasMany(x => x.Days)
				.Table("driver_schedule_item")
				.KeyColumn("driver_schedule_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
		}
	}
}
