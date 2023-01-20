using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class CarBulkGoodsAccountingOperationMap : SubclassMap<CarBulkGoodsAccountingOperation>
	{
		public CarBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(GoodsAccountingOperationType.BulkGoodsAccountingOperation));
			References(x => x.Car).Column("car_id");
		}
	}
}
