using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Mappings
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
			Map(x => x.DocumentType).Column("document_type");
			Map(x => x.QrPositionOnDocument).Column("qr_position");
			Map(x => x.UriForQr).Column("uri_for_qr");
		}
	}
}
