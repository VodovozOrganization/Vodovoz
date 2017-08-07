using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class AtWorkDriverDistrictPriorityMap : ClassMap<AtWorkDriverDistrictPriority>
	{
		public AtWorkDriverDistrictPriorityMap ()
		{
			Table("at_work_driver_district_priorities");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Priority).Column ("priority");

			References(x => x.Driver).Column ("at_work_driver_id").Not.Nullable();
			References(x => x.District).Column ("district_id").Not.LazyLoad().Not.Nullable();
		}
	}
}

