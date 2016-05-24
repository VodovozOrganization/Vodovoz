using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class CarLoadDocumentItemMap : ClassMap<CarLoadDocumentItem>
	{
		public CarLoadDocumentItemMap ()
		{
			Table ("store_car_load_document_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			References (x => x.Document).Column ("car_load_document_id");
			References (x => x.MovementOperation).Column ("warehouse_movement_operation_id");
		}
	}
}

