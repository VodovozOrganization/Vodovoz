using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Accounting
{
	public class PaymentWriteOffMap : ClassMap<PaymentWriteOff>
	{
		public PaymentWriteOffMap()
		{
			Table("payment_write_offs");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.PaymentNumber).Column("payment_number");
			Map(x => x.Reason).Column("reason");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.OrganizationId).Column("organization_id");
			Map(x => x.Sum).Column("sum");
			Map(x => x.FinancialExpenseCategoryId).Column("financial_expense_category_id");
			Map(x => x.Comment).Column("comment");

			References(x => x.CashlessMovementOperation)
				.Column("cashless_movement_operation_id")
				.Not
				.LazyLoad()
				.Cascade
				.AllDeleteOrphan();
		}
	}
}
