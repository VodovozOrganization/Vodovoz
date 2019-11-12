using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class MovementDocumentItemMap : ClassMap<MovementDocumentItem>
	{
		public MovementDocumentItemMap ()
		{
			Table ("store_movement_document_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.SendedAmount).Column("sended_amount");
			Map(x => x.ReceivedAmount).Column ("received_amount");
			References (x => x.Document).Column ("movement_document_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.WarehouseWriteoffOperation).Column("writeoff_movement_operation_id").Cascade.All();
			References (x => x.WarehouseIncomeOperation).Column ("income_movement_operation_id").Cascade.All();
		}
	}
}