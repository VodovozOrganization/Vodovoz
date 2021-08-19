using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorMap : ClassMap<Sector>
	{
		public SectorMap()
		{
			Table("sectors");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DateCreated).Column("create_date");
			
			HasMany(x => x.SectorVersions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_id").LazyLoad();
			HasMany(x => x.SectorDeliveryRuleVersions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_id").LazyLoad();
			HasMany(x => x.SectorWeekDaySchedulesVersions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_id").LazyLoad();
			HasMany(x => x.SectorWeekDayDeliveryRuleVersions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_id").LazyLoad();
			HasMany(x => x.DeliveryPointSectorVersions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_id").LazyLoad();
		}
	}
}
