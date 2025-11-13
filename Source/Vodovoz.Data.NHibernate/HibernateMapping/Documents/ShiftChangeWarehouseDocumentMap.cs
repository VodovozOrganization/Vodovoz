using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class ShiftChangeWarehouseDocumentMap : ClassMap<ShiftChangeWarehouseDocument>
	{
		public ShiftChangeWarehouseDocumentMap()
		{
			Table("store_shiftchange");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Map(x => x.Comment).Column("comment");
			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.ShiftChangeResidueDocumentType).Column("shift_change_residue_document_type");
			Map(x => x.SortedByNomenclatureName).Column("sorted_by_nomenclature_name");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.Warehouse).Column("warehouse_id");
			References(x => x.Car).Column("car_id");
			References(x => x.Sender).Column("sender_id");
			References(x => x.Receiver).Column("receiver_id");

			HasMany(x => x.NomenclatureItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("store_shiftchange_id");
			HasMany(x => x.InstanceItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("store_shiftchange_id");
		}
	}
}
