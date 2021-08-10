using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorWeekDayScheduleVersionMap : ClassMap<SectorWeekDayScheduleVersion>
	{
		public SectorWeekDayScheduleVersionMap()
		{
			Table("sector_week_days_version");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");

			References(x => x.Sector).Column("sector_id");
			
			HasMany(x => x.SectorSchedules).KeyColumn("sector_week_days_schedule_version_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}