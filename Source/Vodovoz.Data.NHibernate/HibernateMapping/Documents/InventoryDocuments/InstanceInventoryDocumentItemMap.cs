using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.InventoryDocuments;

namespace Vodovoz.HibernateMapping.Documents.InventoryDocuments
{
	public class InstanceInventoryDocumentItemMap : ClassMap<InstanceInventoryDocumentItem>
	{
		public InstanceInventoryDocumentItemMap()
		{
			Table("store_inventory_instance_item");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.AmountInDB).Column("amount_in_db");
			Map(x => x.IsMissing).Column("is_missing");
			Map(x => x.Comment).Column("comment");

			References(x => x.Fine).Column("fine_id");
			References(x => x.Document).Column("store_inventory_id").Not.Nullable();
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id").Not.Nullable();
		}
	}
}
