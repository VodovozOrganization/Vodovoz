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
			Map(x => x.TimeForTransferToManualProcessingWithoutFastDelivery)
				.Column("time_for_transfer_to_manual_processing_without_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
			Map(x => x.TimeForTransferToManualProcessingWithFastDelivery)
				.Column("time_for_transfer_to_manual_processing_with_fast_delivery")
				.CustomType<TimeAsTimeSpanType>()
				.Not.Nullable();
		}
	}
}
