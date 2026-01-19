using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
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
