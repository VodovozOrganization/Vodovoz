using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class DriverWorkScheduleSetMap : ClassMap<DriverWorkScheduleSet>
	{
		public DriverWorkScheduleSetMap()
		{
			Table("driver_work_schedule_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DateActivated).Column("date_activated");
			Map(x => x.DateDeactivated).Column("date_deactivated");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.IsCreatedAutomatically).Column("is_created_automatically");

			References(x => x.Driver).Column("driver_id");
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditor).Column("last_editor_id");

			HasMany(x => x.DriverWorkSchedules)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("driver_work_schedule_set_id");
		}
	}
}
