using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class CarInstanceGoodsAccountingOperationMap : SubclassMap<CarInstanceGoodsAccountingOperation>
	{
		public CarInstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(GoodsAccountingOperationType.InstanceGoodsAccountingOperation));
			References(x => x.Car).Column("car_id");
		}
	}
}
