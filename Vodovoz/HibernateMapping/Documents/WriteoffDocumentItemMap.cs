using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class WriteoffDocumentItemMap : ClassMap<WriteoffDocumentItem>
	{
		public WriteoffDocumentItemMap ()
		{
			Table ("writeoff_document_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			References (x => x.Document).Column ("writeoff_document_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.CullingCategory).Column ("culling_category_id");
			References (x => x.WarehouseWriteoffOperation).Column ("writeoff_warehouse_movement_operation_id").Cascade.All ();
			References (x => x.CounterpartyWriteoffOperation).Column ("writeoff_counterparty_movement_operation_id").Cascade.All ();
		}
	}
}