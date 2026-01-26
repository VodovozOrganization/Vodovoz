using FluentNHibernate.Mapping;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.FastPayments
{
	public class WrongSignatureFromReceivedFastPaymentEventMap : ClassMap<WrongSignatureFromReceivedFastPaymentEvent>
	{
		public WrongSignatureFromReceivedFastPaymentEventMap()
		{
			Table("wrong_signatures_from_received_fast_payments_events");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.FastPaymentId).Column("fast_payment_id");
			Map(x => x.BankSignature).Column("bank_signature");
			Map(x => x.GeneratedSignature).Column("generated_signature");
			Map(x => x.OrderNumber).Column("order_number");
			Map(x => x.SentDate).Column("sent_date");
			Map(x => x.ShopId).Column("shop_id");
		}
	}
}
