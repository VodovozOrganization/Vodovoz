using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Schemas.Logistics;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class CompletedDriverWarehouseEventMap : ClassMap<CompletedDriverWarehouseEvent>
	{
		public CompletedDriverWarehouseEventMap()
		{
			Table(CompletedDriverWarehouseEventSchema.TableName);
			
			Id(x => x.Id)
				.Column(CompletedDriverWarehouseEventSchema.IdColumn).GeneratedBy.Native();

			Map(x => x.Latitude).Column(CompletedDriverWarehouseEventSchema.LatitudeColumn);
			Map(x => x.Longitude).Column(CompletedDriverWarehouseEventSchema.LongitudeColumn);
			Map(x => x.CompletedDate).Column(CompletedDriverWarehouseEventSchema.CompletedColumn);
			Map(x => x.DistanceMetersFromScanningLocation)
				.Column(CompletedDriverWarehouseEventSchema.DistanceMetersFromScanningLocationColumn);
			Map(x => x.DocumentId).Column(CompletedDriverWarehouseEventSchema.DocumentIdColumn);

			References(x => x.DriverWarehouseEvent)
				.Column(CompletedDriverWarehouseEventSchema.DriverWarehouseEventColumn);
			References(x => x.Employee)
				.Column(CompletedDriverWarehouseEventSchema.EmployeeColumn);
			References(x => x.Car)
				.Column(CompletedDriverWarehouseEventSchema.CarColumn);
		}
	}
}
