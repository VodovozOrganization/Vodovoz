using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HMap
{
	public class DepositOperationMap: ClassMap<DepositOperation>
	{
		public DepositOperationMap ()
		{
			Table ("deposit_operations");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.ReceivedDeposit).Column ("received_deposit");
			Map (x => x.RefundDeposit).Column ("refund_deposit");
			Map (x => x.OperationTime).Column ("operation_time");
			Map (x => x.DepositType).Column ("deposit_type").CustomType<DepositTypeStringType> ();
			References (x => x.Order).Column ("order_id");
			References (x => x.Counterparty).Column ("counterparty_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.OrderItem).Column ("order_item_id");
		}
	}
}

