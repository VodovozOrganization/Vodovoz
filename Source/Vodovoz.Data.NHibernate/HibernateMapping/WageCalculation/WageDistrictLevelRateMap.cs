using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation
{
	public class WageDistrictLevelRateMap : ClassMap<WageDistrictLevelRate>
	{
		public WageDistrictLevelRateMap()
		{
			Table("wage_district_level_rate");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CarTypeOfUse).Column("car_type_of_use");

			References(x => x.WageDistrict).Column("wage_district_id");
			References(x => x.WageDistrictLevelRates).Column("wage_district_level_rates_id");

			HasMany(x => x.WageRates).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("wage_district_level_rate_id").OrderBy("wage_rate_type");
		}
	}
}
