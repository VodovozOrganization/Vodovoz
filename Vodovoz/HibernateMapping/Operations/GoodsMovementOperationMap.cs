using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HMap
{
	public class GoodsMovementOperationMap : ClassMap<GoodsMovementOperation>
	{
		public GoodsMovementOperationMap ()
		{
			Table ("goods_movement_operations");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OperationTime).Column ("operation_time").Not.Nullable ();
			Map (x => x.Amount).Column ("amount");
			Map (x => x.Sale).Column ("sale");
			References (x => x.Order).Column ("order_id");
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.WriteoffWarehouse).Column ("writeoff_warehouse_id");
			References (x => x.IncomingWarehouse).Column ("incoming_warehouse_id");
			References (x => x.WriteoffCounterparty).Column ("writeoff_counterparty_id");
			References (x => x.IncomingCounterparty).Column ("incoming_counterparty_id");
			References (x => x.WriteoffDeliveryPoint).Column ("writeoff_delivery_point_id");
			References (x => x.IncomingDeliveryPoint).Column ("incoming_delivery_point_id");
		}
	}
}

