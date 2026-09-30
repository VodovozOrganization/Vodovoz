using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.FastPayments;

namespace Vodovoz.Core.Data.NHibernate.Mapping.FastPayments
{
	public class FastPaymentMap : ClassMap<FastPaymentEntity>
	{
		public FastPaymentMap()
		{
			Table("fast_payments");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.FastPaymentStatus)
				.Column("payment_status")
				.CustomType<EnumStringType<FastPaymentStatus>>();

			References(x => x.Order)
				.Column("order_id");
		}
	}
}
