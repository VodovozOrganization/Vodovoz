using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class SelfDeliveryDocumentReturnedMap : ClassMap<SelfDeliveryDocumentReturned>
	{
		public SelfDeliveryDocumentReturnedMap ()
		{
			Table ("store_self_delivery_document_returned");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.Amount).Column("amount");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Document).Column ("store_self_delivery_document_id");
			References (x => x.WarehouseMovementOperation).Column ("warehouse_movement_operation_id").Cascade.All();
		}
	}
}

