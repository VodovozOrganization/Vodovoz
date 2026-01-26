using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Core.Data.NHibernate.Operations
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
