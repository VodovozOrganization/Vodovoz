using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class DriverDistrictPriorityMap : ClassMap<DriverDistrictPriority>
	{
		public DriverDistrictPriorityMap ()
		{
			Table("driver_district_priorities");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Priority).Column ("priority");

			References(x => x.Driver).Column ("driver_id").Not.Nullable();
			References(x => x.District).Column ("district_id").Not.LazyLoad().Not.Nullable();
		}
	}
}

