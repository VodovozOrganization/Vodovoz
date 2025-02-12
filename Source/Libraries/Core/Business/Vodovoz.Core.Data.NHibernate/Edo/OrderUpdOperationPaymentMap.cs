using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class OrderUpdOperationPaymentMap : ClassMap<OrderUpdOperationPayment>
	{
		public OrderUpdOperationPaymentMap()
		{
			Table("order_upd_operation_payments");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PaymentNum).Column("payment_num");
			Map(x => x.PaymentDate).Column("payment_date");

			References(x => x.OrderUpdOperation).Column("order_upd_operation_id");
		}
	}
}
