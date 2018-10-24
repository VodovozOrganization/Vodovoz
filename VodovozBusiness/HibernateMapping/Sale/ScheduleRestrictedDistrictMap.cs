using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping
{
	public class ScheduleRestrictedDistrictMap: ClassMap<ScheduleRestrictedDistrict>
	{
		public ScheduleRestrictedDistrictMap()
		{
			Table("schedule_restricted_districts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DistrictName).Column("district_name");
			Map(x => x.MinBottles).Column("min_bottles");
			Map(x => x.DistrictBorder).Column("district_border").CustomType<GeometryType>();
			Map(x => x.WaterPrice).Column("water_price");
			Map(x => x.PriceType).Column("price_type").CustomType<DistrictWaterPriceStringType>();

			References(x => x.ScheduleRestrictionMonday).Column("monday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionTuesday).Column("tuesday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionWednesday).Column("wednesday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionThursday).Column("thursday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionFriday).Column("friday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionSaturday).Column("saturday_restriction_id").Cascade.All();
			References(x => x.ScheduleRestrictionSunday).Column("sunday_restriction_id").Cascade.All();
		}
	}
}
