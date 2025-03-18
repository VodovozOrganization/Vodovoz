using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class DistrictMap : ClassMap<District>
	{
		public DistrictMap()
		{
			Table("districts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DistrictName).Column("district_name");
			Map(x => x.MinBottles).Column("min_bottles");
			Map(x => x.DistrictBorder).Column("district_border").CustomType<MySQL57GeometryType>();
			Map(x => x.WaterPrice).Column("water_price");
			Map(x => x.PriceType).Column("price_type");

			References(x => x.TariffZone).Column("tariff_zone_id");
			References(x => x.WageDistrict).Column("wage_district_id");
			References(x => x.GeographicGroup).Column("geo_group_id");
			References(x => x.DistrictsSet).Column("districts_set_id");
			References(x => x.CopyOf).Column("copy_of");

			HasMany(x => x.DistrictCopyItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id");

			HasMany(x => x.AllDistrictRuleItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id");

			HasMany(x => x.AllDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id");
		}
	}
}
