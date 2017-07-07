using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class SelfDeliveryDocumentItemMap : ClassMap<SelfDeliveryDocumentItem>
	{
		public SelfDeliveryDocumentItemMap ()
		{
			Table ("store_self_delivery_document_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.Amount).Column("amount");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.OrderItem).Column ("order_item_id");
			References (x => x.Document).Column ("store_self_delivery_document_id");
			References (x => x.WarehouseMovementOperation).Column ("warehouse_movement_operation_id").Cascade.All();
		}
	}
}

