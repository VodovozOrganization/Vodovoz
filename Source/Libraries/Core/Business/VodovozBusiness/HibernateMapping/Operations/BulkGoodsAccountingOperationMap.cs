using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class BulkGoodsAccountingOperationMap : SubclassMap<BulkGoodsAccountingOperation>
	{
		public BulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(GoodsAccountingOperationType.BulkGoodsAccountingOperation));
		}
	}
}
