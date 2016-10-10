using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HMap
{
	public class FuelOperationMap: ClassMap<FuelOperation>
	{
		public FuelOperationMap ()
		{
			Table ("fuel_operations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.OperationTime).Column ("date");
			Map (x => x.LitersGived).Column ("liters_gived");
			Map (x => x.LitersOutlayed).Column ("liters_outlayed");

			References (x => x.Fuel).Column ("fuel_type_id");
			References (x => x.Driver).Column ("driver_id");
		}
	}
}

