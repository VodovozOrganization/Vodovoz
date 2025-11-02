using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderMap : ClassMap<OnlineOrder>
	{
		public OnlineOrderMap()
		{
			Table("online_orders");
			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Source).Column("source");
			Map(x => x.CounterpartyId).Column("first_counterparty_id");
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
			Map(x => x.ExternalOrderId).Column("external_online_order_id");
			Map(x => x.DeliveryPointId).Column("first_delivery_point_id");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");
			Map(x => x.SelfDeliveryGeoGroupId).Column("first_self_delivery_geo_group_id");
			Map(x => x.OnlineOrderPaymentType).Column("online_order_payment_type");
			Map(x => x.OnlineOrderPaymentStatus).Column("online_order_payment_status");
			Map(x => x.OnlineOrderStatus).Column("online_order_status");
			Map(x => x.OnlinePayment).Column("online_payment");
			Map(x => x.OnlinePaymentSource).Column("online_payment_source");
			Map(x => x.IsNeedConfirmationByCall).Column("is_need_confirmation_by_call");
			Map(x => x.DeliveryDate).Column("delivery_date");
			Map(x => x.Created).Column("created");
			Map(x => x.DeliveryScheduleId).Column("first_delivery_schedule_id");
			Map(x => x.IsFastDelivery).Column("is_fast_delivery");
			Map(x => x.ContactPhone).Column("contact_phone");
			Map(x => x.OnlineOrderComment).Column("online_order_comment");
			Map(x => x.Trifle).Column("trifle");
			Map(x => x.BottlesReturn).Column("bottles_return");
			Map(x => x.OnlineOrderSum).Column("online_order_sum");
			Map(x => x.CallBeforeArrivalMinutes).Column("call_before_arrival_minutes");
			Map(x => x.DontArriveBeforeInterval).Column("dont_arrive_before_interval");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.DeliverySchedule).Column("delivery_schedule_id");
			References(x => x.SelfDeliveryGeoGroup).Column("self_delivery_geo_group_id");
			References(x => x.EmployeeWorkWith).Column("employee_work_with_id");
			References(x => x.OnlineOrderCancellationReason).Column("online_order_cancellation_reason_id");

			HasMany(x => x.Orders)
				.KeyColumn("online_order_id")
				.Cascade.AllDeleteOrphan();

			HasMany(x => x.OnlineOrderItems)
				.KeyColumn("online_order_id")
				.Inverse().Cascade.AllDeleteOrphan();
			
			HasMany(x => x.OnlineRentPackages)
				.KeyColumn("online_order_id")
				.Inverse().Cascade.AllDeleteOrphan();
		}
	}
}
