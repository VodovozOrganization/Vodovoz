using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Payments
{
	public class RefundOperationMap : ClassMap<RefundOperation>
	{
		public RefundOperationMap()
		{
			Table("refund_operations");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.IdempotenceKey)
				.Column("idempotence_key")
				.Unique();

			Map(x => x.OnlineOrderId)
				.Column("online_order_id");

			Map(x => x.TransactionId)
				.Column("transaction_id");

			Map(x => x.RefundId)
				.Column("refund_id");

			Map(x => x.PaymentSource)
				.Column("payment_source");

			Map(x => x.CreatedAt)
				.Column("created_at");

			Map(x => x.IsSuccess)
				.Column("is_success");

			Map(x => x.ErrorMessage)
				.Column("error_message");
		}
	}
}
