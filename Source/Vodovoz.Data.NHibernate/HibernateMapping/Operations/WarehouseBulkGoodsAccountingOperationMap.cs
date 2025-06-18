using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class WarehouseBulkGoodsAccountingOperationMap : SubclassMap<WarehouseBulkGoodsAccountingOperation>
	{
		public WarehouseBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.WarehouseBulkGoodsAccountingOperation));
			References(x => x.Warehouse).Column("warehouse_id");
		}
	}
}
