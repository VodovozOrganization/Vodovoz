using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class CarBulkGoodsAccountingOperationMap : SubclassMap<CarBulkGoodsAccountingOperation>
	{
		public CarBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationTypeByStorage.Car));
			References(x => x.Car).Column("car_id");
		}
	}
}
