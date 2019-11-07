using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.HibernateMapping.WageCalculation.AdvancedWageParameters
{
	public class DeliveryTimeAdvancedWageParameterMap : SubclassMap<DeliveryTimeAdvancedWageParameter>
	{
		public DeliveryTimeAdvancedWageParameterMap()
		{
			DiscriminatorValue(AdvancedWageParameterType.DeliveryTime.ToString());

			Map(x => x.StartTime).Column("start_time");
			Map(x => x.EndTime).Column("end_time");
		}
	}
}
