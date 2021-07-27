using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class DeliveryPointSectorVersionMap: ClassMap<DeliveryPointSectorVersion>
	{
		public DeliveryPointSectorVersionMap()
		{
			Table("delivery_points_geodata_versions");
			Not.LazyLoad();
			
			Id(x => x).Column("id").GeneratedBy.Native();

			Map(x => x.Latitude).Column("latitude");
			Map(x => x.Longitude).Column("longitude");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.DistanceFromBaseMeters).Column("distance_from_center_meters");
			
			References(x => x.Sector).Column("sector_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
		}
	}
}