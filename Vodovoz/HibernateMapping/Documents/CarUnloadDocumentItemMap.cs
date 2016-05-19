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
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			References (x => x.Document).Column ("car_unload_document_id");
			References (x => x.MovementOperation).Column ("warehouse_movement_operation_id");
		}
	}
}

