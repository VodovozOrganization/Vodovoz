using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class MovementDocumentItemMap : ClassMap<MovementDocumentItem>
	{
		public MovementDocumentItemMap ()
		{
			Table ("movement_document_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			References (x => x.Document).Column ("movement_document_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.MoveGoodsOperation).Column ("goods_movement_operation_id").Not.Nullable ().Cascade.All ();
		}
	}
}