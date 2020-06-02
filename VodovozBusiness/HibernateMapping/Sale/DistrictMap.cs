using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping
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
			Map(x => x.PriceType).Column("price_type").CustomType<DistrictWaterPriceStringType>();
			
			References(x => x.TariffZone).Column("tariff_zone_id");
			References(x => x.WageDistrict).Column("wage_district_id");

			HasMany(x => x.CommonDistrictRuleItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id");

			HasMany(x => x.TodayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Today}'");
			HasMany(x => x.MondayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Monday}'");
			HasMany(x => x.TuesdayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Tuesday}'");
			HasMany(x => x.WednesdayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Wednesday}'");
			HasMany(x => x.ThursdayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Thursday}'");
			HasMany(x => x.FridayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Friday}'");
			HasMany(x => x.SaturdayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Saturday}'");
			HasMany(x => x.SundayDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("district_id")
				.Where($"week_day = '{WeekDayName.Sunday}'");
			
			HasManyToMany(x => x.GeographicGroups).Table("geographic_groups_to_entities")
								.ParentKeyColumn("district_id")
								.ChildKeyColumn("geographic_group_id")
								.LazyLoad();
		}
	}
}
