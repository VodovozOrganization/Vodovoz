using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorWeekDayRulesVersionMap : ClassMap<SectorWeekDayRulesVersion>
	{
		public SectorWeekDayRulesVersionMap()
		{
			Table("sector_week_days_version");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");

			References(x => x.Sector).Column("sector_id");
			
			HasMany(x => x.SectorSchedules).KeyColumn("sector_week_days_version_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
			HasMany(x => x.SectorDeliveryRules)             .KeyColumn("sector_week_days_version_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}