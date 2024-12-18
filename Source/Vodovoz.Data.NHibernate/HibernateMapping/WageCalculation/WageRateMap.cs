using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation
{
	public class WageRateMap : ClassMap<WageRate>
	{
		public WageRateMap()
		{
			Table("wage_rates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WageRateType).Column("wage_rate_type");
			Map(x => x.ForDriverWithForwarder).Column("for_driver_with_forwarder");
			Map(x => x.ForDriverWithoutForwarder).Column("for_driver_without_forwarder");
			Map(x => x.ForForwarder).Column("for_forwarder");

			References(x => x.WageDistrictLevelRate).Column("wage_district_level_rate_id");

			HasMany(x => x.ChildrenParameters).Cascade.AllDeleteOrphan().KeyColumn("wage_rate_id");
		}
	}
}
