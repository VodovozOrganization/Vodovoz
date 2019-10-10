using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class WageDistrictLevelRatesMap : ClassMap<WageDistrictLevelRates>
	{
		public WageDistrictLevelRatesMap()
		{
			Table("wage_district_level_rates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.IsDefaultLevel).Column("is_default_level");

			HasMany(x => x.LevelRates).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("wage_district_level_rates_id");
		}
	}
}
