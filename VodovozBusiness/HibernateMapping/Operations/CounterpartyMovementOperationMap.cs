using System;
using FluentNHibernate.Mapping;

namespace Vodovoz
{
	public class CounterpartyMovementOperationMap : ClassMap<CounterpartyMovementOperation>
	{
		public CounterpartyMovementOperationMap ()
		{
			Table ("counterparty_movement_operations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map (x => x.Amount).Column ("amount");
			Map (x => x.ForRent).Column ("for_rent");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.WriteoffCounterparty).Column ("writeoff_counterparty_id");
			References (x => x.IncomingCounterparty).Column ("incoming_counterparty_id");
			References (x => x.WriteoffDeliveryPoint).Column ("writeoff_delivery_point_id");
			References (x => x.IncomingDeliveryPoint).Column ("incoming_delivery_point_id");
		}
	}
}

