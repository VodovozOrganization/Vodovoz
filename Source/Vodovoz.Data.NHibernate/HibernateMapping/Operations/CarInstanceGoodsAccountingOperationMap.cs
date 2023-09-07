using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class CarInstanceGoodsAccountingOperationMap : SubclassMap<CarInstanceGoodsAccountingOperation>
	{
		public CarInstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.CarInstanceGoodsAccountingOperation));
			References(x => x.Car).Column("car_id");
		}
	}
}
