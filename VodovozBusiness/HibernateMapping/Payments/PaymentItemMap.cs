using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.HibernateMapping.Payments
{
	public class PaymentItemMap : ClassMap<PaymentItem>
	{
		public PaymentItemMap()
		{
			Table("payment_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Sum).Column("sum");

			References(x => x.Order).Column("order_id");
			References(x => x.Payment).Column("payment_id");
		}
	}
}
