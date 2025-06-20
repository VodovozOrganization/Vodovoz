using FluentNHibernate.Mapping;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.InventoryDocuments
{
	public class InventoryDocumentMap : ClassMap<Domain.Documents.InventoryDocuments.InventoryDocument>
	{
		public InventoryDocumentMap()
		{
			Table("store_inventory");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Map(x => x.Comment).Column("comment");
			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.SortedByNomenclatureName).Column("sorted_by_nomenclature_name");
			Map(x => x.InventoryDocumentStatus).Column("document_status");
			Map(x => x.InventoryDocumentType).Column("document_type");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.Warehouse).Column("warehouse_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Car).Column("car_id");

			HasMany(x => x.NomenclatureItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("store_inventory_id");
			HasMany(x => x.InstanceItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("store_inventory_id");
		}
	}
}
