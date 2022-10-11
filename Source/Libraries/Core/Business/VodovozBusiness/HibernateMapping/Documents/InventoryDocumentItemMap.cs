using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class InventoryDocumentItemMap : ClassMap<InventoryDocumentItem>
	{
		public InventoryDocumentItemMap ()
		{
			Table ("store_inventory_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.AmountInDB).Column ("amount_in_db");
			Map (x => x.AmountInFact).Column ("amount_in_fact");
			Map (x => x.Comment).Column ("comment");
			//Map (x => x.SumOfDamage).Column("sum_of_damage");
			References (x => x.Fine).Column ("fine_id");
			References (x => x.Document).Column ("store_inventory_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.WarehouseChangeOperation).Column ("warehouse_movement_operation_id").Cascade.All ();
		}
	}
}