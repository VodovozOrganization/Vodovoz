using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class RegradingOfGoodsDocumentItemMap : ClassMap<RegradingOfGoodsDocumentItem>
	{
		public RegradingOfGoodsDocumentItemMap ()
		{
			Table ("store_regrading_of_goods_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			Map (x => x.Comment).Column ("comment");
			References(x => x.Fine).Column("fine_id");
			References (x => x.Document).Column ("store_regrading_of_goods_id").Not.Nullable ();
			References (x => x.NomenclatureOld).Column ("nomenclature_old_id").Not.Nullable ();
			References (x => x.NomenclatureNew).Column ("nomenclature_new_id").Not.Nullable ();
			References (x => x.WarehouseWriteOffOperation).Column ("warehouse_writeoff_operation_id").Cascade.All ();
			References (x => x.WarehouseIncomeOperation).Column ("warehouse_income_operation_id").Cascade.All ();
		}
	}
}