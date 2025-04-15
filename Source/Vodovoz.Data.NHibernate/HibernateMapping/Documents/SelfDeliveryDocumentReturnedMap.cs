using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class SelfDeliveryDocumentReturnedMap : ClassMap<SelfDeliveryDocumentReturned>
	{
		public SelfDeliveryDocumentReturnedMap()
		{
			Table("store_self_delivery_document_returned");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Amount).Column("amount");

			Map(x => x.Direction).Column("direction");
			Map(x => x.DirectionReason).Column("direction_reason");
			Map(x => x.OwnType).Column("own_type");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.Document).Column("store_self_delivery_document_id");
			References(x => x.CounterpartyMovementOperation).Column("counterparty_movement_operation_id").Cascade.All();
			References(x => x.GoodsAccountingOperation).Column("warehouse_movement_operation_id").Cascade.All();
		}
	}
}
