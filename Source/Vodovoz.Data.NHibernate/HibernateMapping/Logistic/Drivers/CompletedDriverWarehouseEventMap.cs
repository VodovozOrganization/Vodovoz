using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class CompletedDriverWarehouseEventMap : ClassMap<CompletedDriverWarehouseEvent>
	{
		public CompletedDriverWarehouseEventMap()
		{
			Table("completed_drivers_warehouses_events");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Latitude).Column("latitude");
			Map(x => x.Longitude).Column("longitude");
			Map(x => x.CompletedDate).Column("completed_date");
			Map(x => x.DistanceMetersFromScanningLocation)
				.Column("distance_meters_from_scanning_location");
			Map(x => x.DocumentType).Column("document_type");
			Map(x => x.DocumentId).Column("document_id");

			References(x => x.DriverWarehouseEvent).Column("driver_warehouse_event_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Car).Column("car_id");
		}
	}
}
