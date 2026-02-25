using FluentNHibernate.Mapping;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.FastPayments
{
	public class FastPaymentMap : ClassMap<FastPayment>
	{
		public FastPaymentMap()
		{
			Table("fast_payments");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FastPaymentStatus).Column("payment_status").CustomType<FastPaymentStatusStringType>();
			Map(x => x.Amount).Column("amount");
			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.PaidDate).Column("paid_date");
			Map(x => x.Ticket).Column("ticket");
			Map(x => x.QRPngBase64).Column("qr_code");
			Map(x => x.ExternalId).Column("external_id");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.FastPaymentGuid).Column("payment_guid");
			Map(x => x.OnlineOrderId).Column("online_order_id");
			Map(x => x.FastPaymentPayType).Column("pay_type").CustomType<FastPaymentPayTypeStringType>();
			Map(x => x.CallbackUrlForMobileApp).Column("callback_url_for_mobile_app");
			Map(x => x.CallbackUrlForAiBot).Column("callback_url_for_ai_bot");
			Map(x => x.PaymentType).Column("payment_type");

			References(x => x.Order).Column("order_id");
			References(x => x.Organization).Column("organization_id");
			References(x => x.PaymentByCardFrom).Column("payment_from_id");
		}
	}
}
