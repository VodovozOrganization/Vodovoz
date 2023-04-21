using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class WarehouseBulkGoodsAccountingOperationMap : SubclassMap<WarehouseBulkGoodsAccountingOperation>
	{
		public WarehouseBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationTypeByStorage.Warehouse));
			References(x => x.Warehouse).Column("warehouse_id");
		}
	}
}
