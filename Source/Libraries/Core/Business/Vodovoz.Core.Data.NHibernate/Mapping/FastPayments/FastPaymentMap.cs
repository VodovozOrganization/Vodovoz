using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.FastPayments;

namespace Vodovoz.Core.Data.NHibernate.Mapping.FastPayments
{
	public class FastPaymentMap : ClassMap<FastPaymentEntity>
	{
		public FastPaymentMap()
		{
			Table("fast_payments");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.PaymentStatus).Column("payment_status");
		}
	}
}
