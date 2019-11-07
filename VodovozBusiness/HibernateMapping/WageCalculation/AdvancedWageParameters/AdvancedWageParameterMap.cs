using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.HibernateMapping.WageCalculation.AdvancedWageParameters
{
	public class AdvancedWageParameterMap : ClassMap<AdvancedWageParameter>
	{
		public AdvancedWageParameterMap()
		{
			Table("advanced_wage_parameter");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
			Map(x => x.AdvancedWageParameterType).Column("type").CustomType<AdvancedWageParameterStringType>();

			Map(x => x.Wage).Column("wage");
			References(x => x.ParentParameter).Column("parent_id");
			References(x => x.WageRate).Column("wage_rate_id");
		}
	}
}
