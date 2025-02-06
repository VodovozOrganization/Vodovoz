using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashlessRequestMap : SubclassMap<CashlessRequest>
	{
		public CashlessRequestMap()
		{
			DiscriminatorValue(PayoutRequestDocumentType.CashlessRequest.ToString());

			// TODO: Добавить в таблицу
			Map(clr => clr.FinancialResponsibilityCenterId).Column("financial_responsibility_center_id");
			Map(clr => clr.PaymentDatePlanned).Column("payment_date_planned");
			Map(clr => clr.OurOrganizationBankAccountId).Column("our_organization_bank_account_id");
			Map(clr => clr.SupplierBankAccountId).Column("supplier_bank_account_id");
			Map(clr => clr.BillNumber).Column("bill_number");
			Map(clr => clr.BillDate).Column("bill_date");
			Map(clr => clr.Sum).Column("sum");
			Map(clr => clr.VatValue).Column("vat_value");
			Map(clr => clr.PaymentPurpose).Column("payment_purpose");
			Map(clr => clr.IsImidiatelyBill).Column("is_imidiately_bill");

			References(clr => clr.Counterparty).Column("counterparty_id");

			HasMany(clr => clr.Comments)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("cashless_request_id");

			HasMany(clr => clr.AttachedFileInformations)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("cashless_request_id");

			HasMany(clr => clr.OutgoingPayments)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("cashless_request_id");
		}
	}
}
