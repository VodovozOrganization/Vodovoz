using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Accounting
{
	public class CashlessIncomeMap : ClassMap<CashlessIncome>
	{
		public CashlessIncomeMap()
		{
			Table("cashless_incomes");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Date).Column("date");
			Map(x => x.Number).Column("number");
			Map(x => x.Total).Column("total_sum");
			Map(x => x.PaymentPurpose).Column("payment_purpose");
			Map(x => x.PayerName).Column("payer_name");
			Map(x => x.PayerInn).Column("payer_inn");
			Map(x => x.PayerKpp).Column("payer_kpp");
			Map(x => x.PayerBank).Column("payer_bank");
			Map(x => x.PayerAcc).Column("payer_account");
			Map(x => x.PayerCurrentAcc).Column("payer_cur_account");
			Map(x => x.PayerCorrespondentAcc).Column("payer_correspondent_account");
			Map(x => x.PayerBankBik).Column("payer_bank_bik");
			Map(x => x.IsManuallyCreated).Column("is_manually_created");
			
			References(x => x.Organization).Column("organization_id");
			References(x => x.OrganizationAccount).Column("organization_account_id");
			
			HasMany(x => x.Payments).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("cashless_income_id");
		}
	}
}
