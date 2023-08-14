using FluentNHibernate.Mapping;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.HibernateMapping.FastPayments
{
	public class FastPaymentNotificationMap : ClassMap<FastPaymentNotification>
	{
		public FastPaymentNotificationMap()
		{
			Table("fast_payment_notifications");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Time).Column("time");
			References(x => x.Payment).Column("payment_id");
			Map(x => x.Type).Column("type");
			Map(x => x.LastTryTime).Column("last_try_time");
			Map(x => x.SuccessfullyNotified).Column("successfully_notified");
			Map(x => x.StopNotifications).Column("stop_notifications");
		}
	}
}
