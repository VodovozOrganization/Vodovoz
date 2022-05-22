using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.HibernateMapping.Payments
{
	public class PaymentFromAvangardMap : ClassMap<PaymentFromAvangard>
	{
		public PaymentFromAvangardMap()
		{
			Table("payments_from_avangard");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.PaidDate).Column("paid_date");
			Map(x => x.OrderNum).Column("order_num");
			Map(x => x.TotalSum).Column("total_sum");
		}
	}
}
