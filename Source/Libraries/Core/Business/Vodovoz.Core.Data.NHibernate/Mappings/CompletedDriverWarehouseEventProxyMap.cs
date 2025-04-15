using FluentNHibernate.Mapping;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain.Schemas.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Mappings
{
	public class CompletedDriverWarehouseEventProxyMap : ClassMap<CompletedDriverWarehouseEventProxy>
	{
		public CompletedDriverWarehouseEventProxyMap()
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
			Map(x => x.CarId).Column(CompletedDriverWarehouseEventSchema.CarColumn);

			References(x => x.DriverWarehouseEvent)
				.Column(CompletedDriverWarehouseEventSchema.DriverWarehouseEventColumn);
			References(x => x.Employee)
				.Column(CompletedDriverWarehouseEventSchema.EmployeeColumn);
		}
	}
}
