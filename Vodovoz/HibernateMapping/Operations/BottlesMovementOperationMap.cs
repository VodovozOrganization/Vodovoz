using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HMap
{
	public class BottlesMovementOperationMap : ClassMap<BottlesMovementOperation>
	{
		public BottlesMovementOperationMap ()
		{
			Table ("bottles_movement_operations");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OperationTime).Column ("operation_time");
			Map (x => x.MovedTo).Column ("moved_to");
			Map (x => x.MovedFrom).Column ("moved_from");
			References (x => x.Order).Column ("order_id");
			References (x => x.Counterparty).Column ("counterparty_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.OrderItem).Column ("order_item_id");
		}
	}
}

