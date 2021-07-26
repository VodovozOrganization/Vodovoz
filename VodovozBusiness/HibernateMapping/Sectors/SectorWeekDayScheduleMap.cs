using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorWeekDayScheduleMap: ClassMap<SectorWeekDaySchedule>
	{
		public SectorWeekDayScheduleMap()
		{
			Table("sector_week_days_delivery_rules");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DeliveryWeekDay).Column("delivery_week_day");
			References(x => x.DeliverySchedule).Column("delivery_schedule_id");

			References(x => x.SectorWeekDayRulesVersion).Column("sector_week_day_rules_version");
		}
	}
}