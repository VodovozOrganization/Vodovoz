using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class CounterpartyMap : ClassMap<Counterparty>
	{
		public CounterpartyMap ()
		{
			Table ("counterparty");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.FullName).Column ("full_name");
			Map (x => x.TypeOfOwnership).Column ("type_of_ownership");
			Map (x => x.Code1c).Column ("code_1c");
			Map (x => x.MaxCredit).Column ("max_credit");
			Map (x => x.Comment).Column ("comment");
			Map (x => x.WaybillComment).Column ("waybill_comment");
			Map (x => x.INN).Column ("inn");
			Map (x => x.KPP).Column ("kpp");
			Map (x => x.JurAddress).Column ("jur_address");
			Map (x => x.Address).Column ("address");
			Map (x => x.PaymentMethod).Column ("payment_method").CustomType<PaymentTypeStringType> ();
			Map (x => x.PersonType).Column ("person_type").CustomType<PersonTypeStringType> ();
			Map (x => x.CounterpartyType).Column ("counterparty_type").CustomType<CounterpartyTypeStringType> ();
			References (x => x.Significance).Column ("significance_id");
			References (x => x.Status).Column ("status_id");
			References (x => x.MainCounterparty).Column ("maincounterparty_id");
			References (x => x.Accountant).Column ("accountant_id");
			References (x => x.SalesManager).Column ("sales_manager_id");
			References (x => x.BottlesManager).Column ("bottles_manager_id");
			References (x => x.MainContact).Column ("main_contact_id");
			References (x => x.FinancialContact).Column ("financial_contact_id");
			References (x => x.DefaultAccount).Column ("default_account_id");
			References (x => x.DefaultExpenseCategory).Column ("default_cash_expense_category_id");
			HasMany (x => x.Phones).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.Accounts).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.DeliveryPoints).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.Emails).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.Contacts).Cascade.AllDeleteOrphan ().LazyLoad ().Inverse ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.CounterpartyContracts).Cascade.None ().LazyLoad ().Inverse ()
				.KeyColumn ("counterparty_id");
			HasMany (x => x.Proxies).Cascade.None ().LazyLoad ().Inverse ()
				.KeyColumn ("counterparty_id");
		}
	}
}

