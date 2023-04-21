using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class CarInstanceGoodsAccountingOperationMap : SubclassMap<CarInstanceGoodsAccountingOperation>
	{
		public CarInstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationTypeByStorage.Car));
			References(x => x.Car).Column("car_id");
		}
	}
}
