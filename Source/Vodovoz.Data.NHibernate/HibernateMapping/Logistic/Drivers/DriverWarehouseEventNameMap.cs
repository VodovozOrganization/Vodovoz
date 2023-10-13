using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverWarehouseEventNameMap : ClassMap<DriverWarehouseEventName>
	{
		public DriverWarehouseEventNameMap()
		{
			Table("drivers_warehouses_events_names");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
