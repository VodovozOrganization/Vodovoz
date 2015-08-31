using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class DeliveryPointMap : ClassMap<DeliveryPoint>
	{
		public DeliveryPointMap ()
		{
			Table ("delivery_points");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.CompiledAddress).Column ("compiled_address");
			Map (x => x.MinutesToUnload).Column ("minutes_to_unload");
			Map (x => x.Floor).Column ("floor");
			Map (x => x.Region).Column ("region");
			Map (x => x.City).Column ("city");
			Map (x => x.Street).Column ("street");
			Map (x => x.Building).Column ("building");
			Map (x => x.Room).Column ("room");
			Map (x => x.Housing).Column ("housing");
			Map (x => x.Letter).Column ("letter");
			Map (x => x.Placement).Column ("placement");
			Map (x => x.Structure).Column ("structure");
			Map (x => x.Comment).Column ("comment");
			Map (x => x.Latitude).Column ("latitude");
			Map (x => x.Longitude).Column ("longitude");
			Map (x => x.IsActive).Column ("is_active");
			Map (x => x.Phone).Column ("phone");
			References (x => x.Contact).Column ("contact_person_id");
			References (x => x.Counterparty).Column ("counterparty_id");
			References (x => x.LogisticsArea).Column ("logistic_area_id");
			References (x => x.DeliverySchedule).Column ("delivery_schedule_id");
		}
	}
}