using FluentNHibernate.Mapping;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Accounting
{
	public class PaymentMap : ClassMap<Payment>
	{
		public PaymentMap()
		{
			Table("payments_from_bank_client");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("payment_date");
			Map(x => x.PaymentNum).Column("payment_num");
			Map(x => x.Total).Column("total_sum");
			Map(x => x.PaymentPurpose).Column("payment_purpose");
			Map(x => x.CounterpartyName).Column("counterparty_name");
			Map(x => x.CounterpartyInn).Column("counterparty_inn");
			Map(x => x.CounterpartyKpp).Column("counterparty_kpp");
			Map(x => x.CounterpartyBank).Column("counterparty_bank");
			Map(x => x.CounterpartyAcc).Column("counterpaty_account");
			Map(x => x.CounterpartyCurrentAcc).Column("counterparty_cur_account");
			Map(x => x.CounterpartyCorrespondentAcc).Column("counterparty_correspondent_account");
			Map(x => x.CounterpartyBik).Column("counterparty_bik");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Status).Column("status");
			Map(x => x.IsManuallyCreated).Column("is_manually_created");
			Map(x => x.RefundPaymentFromOrderId).Column("refund_payment_from_order_id");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.CounterpartyAccount).Column("counterparty_account_id");
			References(x => x.Organization).Column("organization_id");
			References(x => x.OrganizationAccount).Column("organization_account_id");
			References(x => x.ProfitCategory).Column("profit_category_id");
			References(x => x.CashlessMovementOperation).Column("cashless_movement_operation_id")
				.Cascade.AllDeleteOrphan();
			References(x => x.RefundedPayment).Column("refunded_payment_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("payment_id");
		}
	}
}
