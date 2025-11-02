using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverWorkScheduleMap : ClassMap<DriverWorkSchedule>
	{
		public DriverWorkScheduleMap()
		{
			Table("driver_work_schedule");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day");

			References(x => x.DriverWorkScheduleSet).Column("driver_work_schedule_set_id");

			//FIXME Удалить после обновления
			References(x => x.Driver).Column("employee_id");

			References(x => x.DaySchedule).Column("delivery_day_schedule_id");
		}
	}
}
