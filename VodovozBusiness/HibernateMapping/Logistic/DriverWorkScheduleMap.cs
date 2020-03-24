using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class DriverWorkScheduleMap : ClassMap<DriverWorkSchedule>
	{
		public DriverWorkScheduleMap()
		{
			Table("driver_work_schedule");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day").CustomType<WeekDayNameStringType>();
			Map(x => x.AtWork).Column("at_work");

			References(x => x.Employee).Column("employee_id");
			References(x => x.DaySchedule).Column("delivery_day_schedule_id");
		}
	}
}
