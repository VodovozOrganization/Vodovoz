using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class ScheduleRestrictionMap: ClassMap<ScheduleRestriction>
	{
		public ScheduleRestrictionMap()
		{
			Table("schedule_restrictions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day").CustomType<WeekDayNameStringType>();

			References(x => x.District).Column("district_id").Not.Nullable();
			References(x => x.Schedule).Column("schedule_id");
		}
	}
}
