using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping
{
	public class ScheduleRestrictionMap: ClassMap<ScheduleRestriction>
	{
		public ScheduleRestrictionMap()
		{
			Table("schedule_restrictions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day").CustomType<WeekDayNameStringType>();

			HasManyToMany<DeliverySchedule>(x => x.Schedules).Table("delivery_schedules_restrictions")
															.ParentKeyColumn("schedule_restrictions_id")
															.ChildKeyColumn("delivery_schedule_id")
															.LazyLoad();
		}
	}
}
