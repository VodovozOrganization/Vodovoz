using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class WageDistrictLevelRateMap : ClassMap<WageDistrictLevelRate>
	{
		public WageDistrictLevelRateMap()
		{
			Table("wage_district_level_rate");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.WageDistrict).Column("wage_district_id");
			References(x => x.WageDistrictLevelRates).Column("wage_district_level_rates_id");

			HasMany(x => x.WageRates).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("wage_district_level_rate_id");
		}
	}
}
