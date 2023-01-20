using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class WarehouseBulkGoodsAccountingOperationMap : SubclassMap<WarehouseBulkGoodsAccountingOperation>
	{
		public WarehouseBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(GoodsAccountingOperationType.BulkGoodsAccountingOperation));
			References(x => x.Warehouse).Column("warehouse_id");
		}
	}
}
