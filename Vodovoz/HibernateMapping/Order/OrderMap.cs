using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class OrderMap : ClassMap<Order>
	{
		public OrderMap ()
		{
			Table ("orders");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Comment).Column ("comment");
			Map (x => x.DeliveryDate).Column ("delivery_date");
			Map (x => x.SelfDelivery).Column ("self_delivery");
			Map (x => x.BottlesReturn).Column ("bottles_return");
			Map (x => x.SumToReceive).Column ("sum_to_receive");
			Map (x => x.Shipped).Column ("shipped");
			Map (x => x.SumDifferenceReason).Column ("sum_difference_reason");

			Map (x => x.OrderStatus).Column ("order_status").CustomType<OrderStatusStringType> ();
			Map (x => x.SignatureType).Column ("signature_type").CustomType<OrderSignatureTypeStringType> ();
			Map (x => x.PaymentType).Column ("payment_type").CustomType<PaymentTypeStringType> ();

			References (x => x.Client).Column ("client_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.DeliverySchedule).Column ("delivery_schedule_id");
			References (x => x.PreviousOrder).Column ("previous_order_id");
			References (x => x.Contract).Column ("counterparty_contract_id");

			HasMany (x => x.OrderItems).Cascade.AllDeleteOrphan ().Inverse ().LazyLoad ().KeyColumn ("order_id");
			HasMany (x => x.OrderEquipments).Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("order_id");
			HasMany (x => x.OrderDocuments).Cascade.AllDeleteOrphan ().LazyLoad ().Inverse ().KeyColumn ("order_id");
			HasMany (x => x.OrderDepositRefundItem).Cascade.AllDeleteOrphan ().LazyLoad ().Inverse ().KeyColumn ("order_id");
			HasMany (x => x.InitialOrderService).Cascade.None ().LazyLoad ().Inverse ().KeyColumn ("initial_order_id");
			HasMany (x => x.FinalOrderService).Cascade.None ().LazyLoad ().Inverse ().KeyColumn ("final_order_id");
		}
	}
}