using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Accounting
{
	public class OutgoingPaymentMap : ClassMap<OutgoingPayment>
	{
		public OutgoingPaymentMap()
		{
			Table("outgoing_payments");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.PaymentNumber).Column("payment_number");
			Map(x => x.OrganizationId).Column("organization_id");
			Map(x => x.PaymentDate).Column("payment_date");
			Map(x => x.PaymentPurpose).Column("payment_purpose");
			Map(x => x.Sum).Column("sum");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.FinancialExpenseCategoryId).Column("financial_expense_category_id");
			Map(x => x.CashlessRequestId).Column("cashless_request_id");
			Map(x => x.Comment).Column("comment");
		}
	}
}
