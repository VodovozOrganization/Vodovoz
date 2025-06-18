using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class CarBulkGoodsAccountingOperationMap : SubclassMap<CarBulkGoodsAccountingOperation>
	{
		public CarBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.CarBulkGoodsAccountingOperation));
			References(x => x.Car).Column("car_id");
		}
	}
}
