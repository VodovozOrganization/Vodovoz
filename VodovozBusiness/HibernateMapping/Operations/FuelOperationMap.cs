using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping
{
	public class FuelOperationMap: ClassMap<FuelOperation>
	{
		public FuelOperationMap ()
		{
			Table ("fuel_operations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.OperationTime) 	.Column ("date");
			Map (x => x.LitersGived)	.Column ("liters_gived");
			Map (x => x.LitersOutlayed) .Column ("liters_outlayed");
			Map (x => x.IsFine) 		.Column ("is_fine");

			References (x => x.Car)	  .Column ("car_id");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.Fuel)  .Column ("fuel_type_id");
		}
	}
}

