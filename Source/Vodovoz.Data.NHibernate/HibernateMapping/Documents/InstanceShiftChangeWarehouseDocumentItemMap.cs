using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class InstanceShiftChangeWarehouseDocumentItemMap : ClassMap<InstanceShiftChangeWarehouseDocumentItem>
	{
		public InstanceShiftChangeWarehouseDocumentItemMap()
		{
			Table("store_shiftchange_instance_item");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.IsMissing).Column("is_missing");
			Map(x => x.AmountInDB).Column("amount_in_db");
			Map(x => x.Comment).Column("comment");

			References(x => x.Document).Column("store_shiftchange_id").Not.Nullable();
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id").Not.Nullable();
		}
	}
}
