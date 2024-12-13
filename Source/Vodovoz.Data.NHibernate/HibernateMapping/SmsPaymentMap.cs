using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping
{
	public class SmsPaymentMap : ClassMap<SmsPayment>
	{
		public SmsPaymentMap()
		{
			Table("sms_payment");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.SmsPaymentStatus).Column("payment_status");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.Amount).Column("amount");
			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.PaidDate).Column("paid_date");
			Map(x => x.ExternalId).Column("external_id");

			References(x => x.Order).Column("order_id");
			References(x => x.Recepient).Column("recepient_id");
		}
	}
}
