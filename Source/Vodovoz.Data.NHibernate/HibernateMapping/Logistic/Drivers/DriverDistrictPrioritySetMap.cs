using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverDistrictPrioritySetMap : ClassMap<DriverDistrictPrioritySet>
	{
		public DriverDistrictPrioritySetMap()
		{
			Table("driver_district_priority_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DateCreated).Column("date_created");
			Map(x => x.DateLastChanged).Column("date_last_changed");

			Map(x => x.DateActivated).Column("date_activated");
			Map(x => x.DateDeactivated).Column("date_deactivated");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.IsCreatedAutomatically).Column("is_created_automatically");

			References(x => x.Driver).Column("driver_id");
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditor).Column("last_editor_id");

			HasMany(x => x.DriverDistrictPriorities)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("driver_district_priority_set_id")
				.OrderBy("priority ASC");
		}
	}
}
