using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Payments
{
	public class OnlinePaymentMap : ClassMap<OnlinePayment>
	{
		public OnlinePaymentMap()
		{
			Table("online_payments");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.TransactionId)
				.Column("transaction_id");

			Map(x => x.PaymentSource)
				.Column("payment_source");

			Map(x => x.Date)
				.Column("date");
		}
	}
}
