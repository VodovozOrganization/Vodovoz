using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class CounterpartyMap : ClassMap<Vodovoz.Domain.Client.Counterparty>
	{
		public CounterpartyMap()
		{
			Table("counterparty");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Name).Column("name");
			Map(x => x.FullName).Column("full_name");
			Map(x => x.TypeOfOwnership).Column("type_of_ownership");
			Map(x => x.Code1c).Column("code_1c");
			Map(x => x.MaxCredit).Column("max_credit");
			Map(x => x.Comment).Column("comment");
			Map(x => x.INN).Column("inn");
			Map(x => x.KPP).Column("kpp");
			Map(x => x.JurAddress).Column("jur_address");
			Map(x => x.Address).Column("address");
			Map(x => x.SignatoryFIO).Column("signatory_FIO");
			Map(x => x.SignatoryPost).Column("signatory_post");
			Map(x => x.SignatoryBaseOf).Column("signatory_base_of");
			Map(x => x.PhoneFrom1c).Column("phone_from_1c");
			Map(x => x.PaymentMethod).Column("payment_method").CustomType<PaymentTypeStringType>();
			Map(x => x.PersonType).Column("person_type").CustomType<PersonTypeStringType>();
			Map(x => x.NewBottlesNeeded).Column("need_new_bottles");
			Map(x => x.DefaultDocumentType).Column("default_document_type").CustomType<DefaultDocumentTypeStringType>();
			Map(x => x.VodovozInternalId).Column("vod_internal_id");
			Map(x => x.RingUpPhone).Column("ringup_phone");
			References(x => x.MainCounterparty).Column("maincounterparty_id");
			References(x => x.PreviousCounterparty).Column("previous_counterparty_id");
			References(x => x.Accountant).Column("accountant_id");
			References(x => x.SalesManager).Column("sales_manager_id");
			References(x => x.BottlesManager).Column("bottles_manager_id");
			References(x => x.MainContact).Column("main_contact_id");
			References(x => x.FinancialContact).Column("financial_contact_id");
			References(x => x.DefaultExpenseCategory).Column("default_cash_expense_category_id");
			References(x => x.CameFrom).Column("counterparty_camefrom_id");
			References(x => x.FirstOrder).Column("first_order_id");
			HasMany(x => x.Phones).Cascade.All().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.Accounts).Cascade.AllDeleteOrphan().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.DeliveryPoints).Inverse().Cascade.AllDeleteOrphan().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.Emails).Cascade.AllDeleteOrphan().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.Contacts).Cascade.AllDeleteOrphan().LazyLoad().Inverse()
				.KeyColumn("counterparty_id");
			HasMany(x => x.CounterpartyContracts).Cascade.None().LazyLoad().Inverse()
				.KeyColumn("counterparty_id");
			HasMany(x => x.Proxies).Cascade.None().LazyLoad().Inverse()
				.KeyColumn("counterparty_id");
			HasManyToMany(x => x.Tags).Table("counterparty_tags")
									  .ParentKeyColumn("counterparty_id")
									  .ChildKeyColumn("tag_id")
									  .LazyLoad();
		}
	}
}

