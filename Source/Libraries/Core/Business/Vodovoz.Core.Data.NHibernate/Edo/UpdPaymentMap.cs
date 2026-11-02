using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class UpdPaymentMap : ClassMap<UpdPayment>
	{
		public UpdPaymentMap()
		{
			Table("edo_upd_document_payments");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.PaymentNum)
				.Column("payment_num");

			Map(x => x.PaymentDate)
				.Column("payment_date");
		}
	}
}
