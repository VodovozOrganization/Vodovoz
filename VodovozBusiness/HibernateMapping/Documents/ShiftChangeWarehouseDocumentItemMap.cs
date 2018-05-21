using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class ShiftChangeWarehouseDocumentItemMap : ClassMap<ShiftChangeWarehouseDocumentItem>
	{
		public ShiftChangeWarehouseDocumentItemMap()
		{
			Table("store_shiftchange_item");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.AmountInDB).Column("amount_in_db");
			Map(x => x.AmountInFact).Column("amount_in_fact");
			Map(x => x.Comment).Column("comment");
			References(x => x.Document).Column("store_shiftchange_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}
}
