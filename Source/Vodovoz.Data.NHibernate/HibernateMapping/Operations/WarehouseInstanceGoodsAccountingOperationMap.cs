using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class WarehouseInstanceGoodsAccountingOperationMap : SubclassMap<WarehouseInstanceGoodsAccountingOperation>
	{
		public WarehouseInstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.WarehouseInstanceGoodsAccountingOperation));

			References(x => x.Warehouse).Column("warehouse_id");
		}
	}
}
