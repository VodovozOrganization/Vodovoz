using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class SelfDeliveryDocumentItemMap : ClassMap<SelfDeliveryDocumentItem>
	{
		public SelfDeliveryDocumentItemMap ()
		{
			Table ("store_self_delivery_document_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			References (x => x.Document).Column ("store_self_delivery_document_id");
			References (x => x.MovementOperation).Column ("warehouse_movement_operation_id");
		}
	}
}

