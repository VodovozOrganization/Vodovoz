using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.HibernateMapping.WageCalculation.AdvancedWageParameters
{
	public class BottlesCountAdvancedWageParameterMap : SubclassMap<BottlesCountAdvancedWageParameter>
	{
		public BottlesCountAdvancedWageParameterMap()
		{
			DiscriminatorValue(AdvancedWageParameterType.BottlesCount.ToString());

			Map(x => x.BottlesFrom).Column("bottles_from");
			Map(x => x.LeftSing).Column("left_sing").CustomType<ComparisonSingStringType>();
			Map(x => x.RightSing).Column("right_sing").CustomType<ComparisonSingStringType>();
			Map(x => x.BottlesTo).Column("bottles_to");
		}
	}
}
