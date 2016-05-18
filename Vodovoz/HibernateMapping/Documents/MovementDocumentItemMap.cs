using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class MovementDocumentItemMap : ClassMap<MovementDocumentItem>
	{
		public MovementDocumentItemMap ()
		{
			Table ("movement_document_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			References (x => x.Document).Column ("movement_document_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.WarehouseMovementOperation).Column ("warehouse_movement_operation_id").Cascade.All ();
			References (x => x.DeliveryMovementOperation).Column ("delivery_movement_operation_id").Cascade.All ();
			References (x => x.CounterpartyMovementOperation).Column ("counterparty_movement_operation_id").Cascade.All ();
		}
	}
}