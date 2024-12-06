using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class BottlesMovementOperationMap : ClassMap<BottlesMovementOperation>
	{
		public BottlesMovementOperationMap()
		{
			Table("bottles_movement_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Delivered).Column("delivered");
			Map(x => x.DeliveredInDisposableTare).Column("delivered_in_disposable_tare");
			Map(x => x.Returned).Column("returned");
			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
		}
	}
}

