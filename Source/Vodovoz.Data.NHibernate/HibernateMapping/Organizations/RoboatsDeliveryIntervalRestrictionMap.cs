using FluentNHibernate.Mapping;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.HibernateMapping.Organizations
{
	public class RoboatsDeliveryIntervalRestrictionMap : ClassMap<RoboatsDeliveryIntervalRestriction>
	{
		public RoboatsDeliveryIntervalRestrictionMap()
		{
			Table("roboats_delivery_interval_restrictions");

			Id(x => x.Id).GeneratedBy.Native();

			References(x => x.DeliverySchedule).Column("delivery_schedule_id");
			Map(x => x.BeforeAcceptOrderHour).Column("before_accept_order_hour");
		}
	}
}
