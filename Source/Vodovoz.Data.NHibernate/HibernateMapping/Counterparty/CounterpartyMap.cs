using FluentNHibernate.Mapping;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	//TODO: Если не будет обращений по поводу VodInternalId, удалить его из таблицы
	public class CounterpartyMap : ClassMap<Domain.Client.Counterparty>
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
			Map(x => x.IsDeliveriesClosed).Column("is_deliveries_closed");
			Map(x => x.CloseDeliveryDate).Column("close_delivery_date");
			Map(x => x.CloseDeliveryComment).Column("close_delivery_comment");
			Map(x => x.Comment).Column("comment");
			Map(x => x.INN).Column("inn");
			Map(x => x.KPP).Column("kpp");
			Map(x => x.IsLiquidating).Column("is_liquidating");
			Map(x => x.RevenueStatus).Column("revenue_status");
			Map(x => x.OGRN).Column("ogrn");
			Map(x => x.JurAddress).Column("jur_address");
			Map(x => x.Address).Column("address");
			Map(x => x.SignatoryFIO).Column("signatory_FIO");
			Map(x => x.SignatoryPost).Column("signatory_post");
			Map(x => x.SignatoryBaseOf).Column("signatory_base_of");
			Map(x => x.PhoneFrom1c).Column("phone_from_1c");
			Map(x => x.PaymentMethod).Column("payment_method");
			Map(x => x.PersonType).Column("person_type");
			Map(x => x.NewBottlesNeeded).Column("need_new_bottles");
			Map(x => x.DefaultDocumentType).Column("default_document_type");
			Map(x => x.UseSpecialDocFields).Column("use_special_doc_fields");
			Map(x => x.AlwaysPrintInvoice).Column("always_print_invoice");
			Map(x => x.CargoReceiver).Column("special_cargo_receiver");
			Map(x => x.SpecialCustomer).Column("special_customer");
			Map(x => x.SpecialContractName).Column("special_contract_name");
			Map(x => x.SpecialExpireDatePercent).Column("special_expire_date_percent");
			Map(x => x.SpecialExpireDatePercentCheck).Column("special_expire_date_percent_check");
			Map(x => x.PayerSpecialKPP).Column("payer_special_kpp");
			Map(x => x.GovContract).Column("special_gov_contract");
			Map(x => x.SpecialDeliveryAddress).Column("special_delivery_address");
			Map(x => x.OKDP).Column("OKDP");
			Map(x => x.OKPO).Column("OKPO");
			Map(x => x.RingUpPhone).Column("ringup_phone");
			Map(x => x.Torg2Count).Column("torg2_count");
			Map(x => x.TTNCount).Column("ttn_count");
			Map(x => x.UPDCount).Column("upd_count");
			Map(x => x.AllUPDCount).Column("all_upd_count");
			Map(x => x.Torg12Count).Column("torg12_count");
			Map(x => x.ShetFacturaCount).Column("shet_factura_count");
			Map(x => x.CarProxyCount).Column("car_proxy_count");
			Map(x => x.CounterpartyType).Column("counterparty_type");
			Map(x => x.IsChainStore).Column("is_chain_store");
			Map(x => x.IsForRetail).Column("is_for_retail");
			Map(x => x.IsForSalesDepartment).Column("is_for_sales_department");
			Map(x => x.NoPhoneCall).Column("no_phone_call");
			Map(x => x.CargoReceiverSource).Column("cargo_receiver_source");
			Map(x => x.DelayDaysForProviders).Column("delay_days");
			Map(x => x.DelayDaysForBuyers).Column("delay_days_for_buyers");
			Map(x => x.TechnicalProcessingDelay).Column("delay_days_for_technical_processing");
			Map(x => x.TaxType).Column("tax_type");
			Map(x => x.CreateDate).Column("create_date");
			Map(x => x.AlwaysSendReceipts).Column("always_send_receipts");
			Map(x => x.RoboatsExclude).Column("roboats_exclude");
			Map(x => x.ExcludeFromAutoCalls).Column("exclude_from_auto_calls");
			Map(x => x.ReasonForLeaving).Column("reason_for_leaving");
			Map(x => x.RegistrationInChestnyZnakStatus).Column("registration_in_chestny_znak_status");
			Map(x => x.OrderStatusForSendingUpd).Column("order_status_for_sending_upd");
			Map(x => x.IsPaperlessWorkflow).Column("is_paperless_workflow");
			Map(x => x.IsNotSendDocumentsByEdo).Column("is_not_send_documents_by_edo");
			Map(x => x.CanSendUpdInAdvance).Column("can_send_upd_in_advance");
			Map(x => x.SpecialContractNumber).Column("special_contract_number");
			Map(x => x.SpecialContractDate).Column("special_contract_date");
			Map(x => x.DoNotMixMarkedAndUnmarkedGoodsInOrder).Column("do_not_mix_marked_and_unmarked_goods_in_order");
			Map(x => x.Surname).Column("surname");
			Map(x => x.FirstName).Column("first_name");
			Map(x => x.Patronymic).Column("patronymic");
			Map(x => x.NeedSendBillByEdo).Column("need_send_bill_by_edo");
			Map(x => x.DefaultExpenseCategoryId).Column("default_financial_expense_category_id");
			Map(x => x.CloseDeliveryDebtType).Column("close_delivery_debt_type");
			Map(x => x.HideDeliveryPointForBill).Column("hide_delivery_point_for_bill");
			Map(x => x.IsNewEdoProcessing).Column("is_new_edo_processing");

			References(x => x.MainCounterparty).Column("maincounterparty_id");
			References(x => x.PreviousCounterparty).Column("previous_counterparty_id");
			References(x => x.Accountant).Column("accountant_id");
			References(x => x.SalesManager).Column("sales_manager_id");
			References(x => x.BottlesManager).Column("bottles_manager_id");
			References(x => x.MainContact).Column("main_contact_id");
			References(x => x.FinancialContact).Column("financial_contact_id");
			References(x => x.CameFrom).Column("counterparty_camefrom_id");
			References(x => x.FirstOrder).Column("first_order_id");
			References(x => x.CloseDeliveryPerson).Column("close_delivery_employee_id");
			References(x => x.WorksThroughOrganization).Column("works_through_organization_id");
			References(x => x.EdoOperator).Column("edo_operator_id");
			References(x => x.LogisticsRequirements).Column("logistics_requirements_id").Cascade.All();
			References(x => x.CounterpartySubtype).Column("counterparty_subtype_id");
			References(x => x.OurOrganizationAccountForBills).Column("our_organization_account_for_bills");
			References(x => x.Referrer).Column("referrer_id");
			References(x => x.DefaultAccount).Column("default_account_id");

			HasMany(x => x.Phones).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("counterparty_id");
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
			HasMany(x => x.SpecialNomenclatures).Cascade.AllDeleteOrphan().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.NomenclatureFixedPrices).Inverse().Cascade.AllDeleteOrphan().LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.SuplierPriceItems).Cascade.AllDeleteOrphan().LazyLoad().Inverse()
				.KeyColumn("supplier_id");
			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("counterparty_id");

			HasMany(x => x.CounterpartyEdoOperators)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad()
				.KeyColumn("counterparty_id");
			HasMany(x => x.CounterpartyEdoAccounts)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad()
				.KeyColumn("counterparty_id");

			HasManyToMany(x => x.Tags)
				.Table("counterparty_tags")
				.ParentKeyColumn("counterparty_id")
				.ChildKeyColumn("tag_id")
				.LazyLoad();
			HasManyToMany(x => x.SalesChannels)
				.Table("sales_channel_to_counterparty")
				.ParentKeyColumn("counterparty_id")
				.ChildKeyColumn("sales_channel_id")
				.LazyLoad();
		}
	}
}
