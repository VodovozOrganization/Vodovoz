using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverWarehouseEventMap : ClassMap<DriverWarehouseEvent>
	{
		public DriverWarehouseEventMap()
		{
			Table("drivers_warehouses_events");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Latitude).Column("latitude");
			Map(x => x.Longitude).Column("longitude");
			Map(x => x.Type).Column("type");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.EventName).Column("event_name");
		}
	}
}
