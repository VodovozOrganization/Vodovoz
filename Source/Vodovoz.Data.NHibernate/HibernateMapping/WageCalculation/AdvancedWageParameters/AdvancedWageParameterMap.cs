using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation.AdvancedWageParameters
{
	public class AdvancedWageParameterMap : ClassMap<AdvancedWageParameter>
	{
		public AdvancedWageParameterMap()
		{
			Table("advanced_wage_parameter");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
			Map(x => x.AdvancedWageParameterType).Column("type").Update().Not.Insert();

			Map(x => x.ForDriverWithForwarder).Column("for_driver_with_forwarder");
			Map(x => x.ForDriverWithoutForwarder).Column("for_driver_without_forwarder");
			Map(x => x.ForForwarder).Column("for_forwarder");

			References(x => x.ParentParameter).Column("parent_id");
			References(x => x.WageRate).Column("wage_rate_id");
			HasMany(x => x.ChildrenParameters).Cascade.AllDeleteOrphan().KeyColumn("parent_id");
		}
	}
}
