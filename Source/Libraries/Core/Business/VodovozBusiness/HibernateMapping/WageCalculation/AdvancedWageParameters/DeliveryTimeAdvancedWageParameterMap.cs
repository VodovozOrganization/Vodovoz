using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.HibernateMapping.WageCalculation.AdvancedWageParameters
{
	public class DeliveryTimeAdvancedWageParameterMap : SubclassMap<DeliveryTimeAdvancedWageParameter>
	{
		public DeliveryTimeAdvancedWageParameterMap()
		{
			DiscriminatorValue(AdvancedWageParameterType.DeliveryTime.ToString());

			Map(x => x.StartTime).Column("start_time").CustomType<TimeAsTimeSpanType>(); ;
			Map(x => x.EndTime).Column("end_time").CustomType<TimeAsTimeSpanType>(); ;
		}
	}
}
