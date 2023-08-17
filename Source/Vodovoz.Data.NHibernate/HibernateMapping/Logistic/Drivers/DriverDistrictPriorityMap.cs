using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverDistrictPriorityMap : ClassMap<DriverDistrictPriority>
	{
		public DriverDistrictPriorityMap()
		{
			Table("driver_district_priorities");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Priority).Column("priority");

			References(x => x.District).Column("district_id");

			//FIXME Удалить после обновления
			References(x => x.Driver).Column("driver_id");

			References(x => x.DriverDistrictPrioritySet).Column("driver_district_priority_set_id");
		}
	}
}
