using System;
using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;

namespace Vodovoz
{
	public class CarUnloadDocumentItemMap:ClassMap<CarUnloadDocumentItem>
	{
		public CarUnloadDocumentItemMap ()
		{
			Table ("store_car_unload_document_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.ReciveType).Column("receive_type").CustomType<ReciveTypesStringType>();
			References (x => x.Document).Column ("car_unload_document_id");
			References (x => x.MovementOperation).Column ("warehouse_movement_operation_id").Cascade.All();
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}
}

