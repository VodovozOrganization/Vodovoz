using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OnlineOrderTimersMap : ClassMap<OnlineOrderTimers>
	{
		public OnlineOrderTimersMap()
		{
			Table("online_order_timers");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.PayTimeWithFastDelivery)
				.Column("pay_time_with_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();

			Map(x => x.PayTimeWithoutFastDelivery)
				.Column("pay_time_without_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.AutoOrderPayTime)
				.Column("auto_order_pay_time")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.AutoOrderPayTimeByOperator)
				.Column("auto_order_pay_time_by_operator")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.TimeForTransferAutoOrderToManualProcessingFromPaying)
				.Column("time_for_transfer_auto_order_to_manual_processing_from_paying")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();

			Map(x => x.TimeForTransferToManualProcessingWithoutFastDelivery)
				.Column("time_for_transfer_to_manual_processing_without_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();

			Map(x => x.TimeForTransferToManualProcessingWithFastDelivery)
				.Column("time_for_transfer_to_manual_processing_with_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();

			Map(x => x.TimeForWaitingBeforeSendPaymentNotification)
				.Column("time_for_waiting_before_send_payment_notification")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.AutoOrderSettingsTime)
				.Column("auto_order_settings_time")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.AutoOrderSettingsTimeByOperator)
				.Column("auto_order_settings_time_by_operator")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			
			Map(x => x.TimeForTransferAutoOrderToManualProcessingFromSettings)
				.Column("time_for_transfer_auto_order_to_manual_processing_from_settings")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
		}
	}
}
