using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HMap
{
	public class OrderDepositRefundItemMap : ClassMap<OrderDepositRefundItem>
	{
		public OrderDepositRefundItemMap ()
		{
			Table ("order_deposit_refund_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.RefundDeposit).Column ("refund_deposit");
			Map (x => x.DepositType).Column ("deposit_type").CustomType<DepositTypeStringType> ();

			References (x => x.Order).Column ("order_id");
			References (x => x.DepositOperation).Column ("deposit_operation_id");
			References (x => x.PaidRentItem).Column ("paid_rent_equipment_id");
			References (x => x.FreeRentItem).Column ("free_rent_equipment_id");
		}
	}
}