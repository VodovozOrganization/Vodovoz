using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureMinimumBalanceByWarehouseMap : ClassMap<NomenclatureMinimumBalanceByWarehouse>
	{
		public NomenclatureMinimumBalanceByWarehouseMap()
		{
			Table("nomenclature_minimum_balance_by_warehouse");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Warehouse).Column("warehouse_id");

			Map(x => x.MinimumBalance).Column("minimum_balance");
		}
	}
}
