using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorVersionMap: ClassMap<SectorVersion>
	{
		public SectorVersionMap()
		{
			Table("sector_versions");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.MinBottles).Column("min_bottles");
			Map(x => x.WaterPrice).Column("water_price");
			Map(x => x.PriceType).Column("price_type").CustomType<SectorWaterPriceStringType>();
			Map(x => x.Status).Column("status");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Polygon).Column("polygon").CustomType<MySQL57GeometryType>();
			
			References(x => x.Sector).Column("sector_id");
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditor).Column("last_editor_id");
			References(x => x.TariffZone).Column("tariff_zone_id");
			References(x => x.WageSector).Column("wage_district_id");
			References(x => x.GeographicGroup).Column("geographic_group_id");
			
		}
	}
}