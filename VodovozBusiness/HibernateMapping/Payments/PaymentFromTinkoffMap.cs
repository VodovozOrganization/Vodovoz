using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.HibernateMapping.Payments
{
	public class PaymentFromTinkoffMap : ClassMap<PaymentFromTinkoff>
	{
		public PaymentFromTinkoffMap()
		{
			Table("payments_from_tinkoff");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PaymentNr).Column("number");
			Map(x => x.PaymentRUR).Column("total_sum");
			Map(x => x.PaymentStatus).Column("status");
			Map(x => x.DateAndTime).Column("date_and_time");
			Map(x => x.Email).Column("email");
			Map(x => x.Phone).Column("phone");
			Map(x => x.Shop).Column("shop");
		}
	}
}
