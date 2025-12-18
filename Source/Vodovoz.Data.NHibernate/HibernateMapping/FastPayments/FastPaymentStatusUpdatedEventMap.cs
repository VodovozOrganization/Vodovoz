using FluentNHibernate.Mapping;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.FastPayments
{
	public class FastPaymentStatusUpdatedEventMap : ClassMap<FastPaymentStatusUpdatedEvent>
	{
		public FastPaymentStatusUpdatedEventMap()
		{
			Table("fast_payments_status_updated_events");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.CreateAt).Column("create_at");
			Map(x => x.DriverNotified).Column("driver_notified");
			Map(x => x.FastPaymentStatus).Column("fast_payment_status");
			Map(x => x.HttpCode).Column("http_code");
			
			References(x => x.FastPayment).Column("fast_payment_id");
		}
	}
}
