using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping
{
	public class WarehouseMovementOperationMap : ClassMap<WarehouseMovementOperation>
	{
		public WarehouseMovementOperationMap ()
		{
			Table ("warehouse_movement_operations");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map (x => x.Amount).Column ("amount");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.WriteoffWarehouse).Column ("writeoff_warehouse_id");
			References (x => x.IncomingWarehouse).Column ("incoming_warehouse_id");
		}
	}
}

