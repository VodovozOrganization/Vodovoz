using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class OrderUpdOperationMap : ClassMap<OrderUpdOperation>
	{
		public OrderUpdOperationMap()
		{
			Table("order_upd_operations");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.CounterpartyExternalOrderId).Column("counterparty_external_order_id");
			Map(x => x.IsOrderForOwnNeeds).Column("is_order_for_own_needs");
			Map(x => x.OrderDeliveryDate).Column("order_delivery_date");
			Map(x => x.ClientContractDocumentName).Column("client_contract_document_name");
			Map(x => x.ClientContractNumber).Column("client_contract_number");
			Map(x => x.ClientContractDate).Column("client_contract_date");
			Map(x => x.ClientId).Column("client_id");
			Map(x => x.ClientName).Column("client_name");
			Map(x => x.ClientAddress).Column("client_address");
			Map(x => x.ClientGovContract).Column("client_gov_contract");
			Map(x => x.ClientInn).Column("client_inn");
			Map(x => x.ClientKpp).Column("client_kpp");
			Map(x => x.ClientPersonalAccountIdInEdo).Column("client_personal_account_id_in_edo");
			Map(x => x.ConsigneeName).Column("consignee_name");
			Map(x => x.ConsigneeAddress).Column("consignee_address");
			Map(x => x.ConsigneeInn).Column("consignee_inn");
			Map(x => x.ConsigneeKpp).Column("consignee_kpp");
			Map(x => x.ConsigneeSummary).Column("consignee_summary");
			Map(x => x.OrganizationName).Column("organization_name");
			Map(x => x.OrganizationAddress).Column("organization_address");
			Map(x => x.OrganizationInn).Column("organization_inn");
			Map(x => x.OrganizationKpp).Column("organization_kpp");
			Map(x => x.OrganizationTaxcomEdoAccountId).Column("organization_taxcom_edo_account_id");
			Map(x => x.BuhLastName).Column("buh_last_name");
			Map(x => x.BuhName).Column("buh_name");
			Map(x => x.BuhPatronymic).Column("buh_patronymic");
			Map(x => x.LeaderLastName).Column("leader_last_name");
			Map(x => x.LeaderName).Column("leader_name");
			Map(x => x.LeaderPatronymic).Column("leader_patronymic");
			Map(x => x.BottlesInFact).Column("bottles_in_fact");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");

			HasMany(x => x.Payments).Cascade.AllDeleteOrphan().Inverse().KeyColumn("order_upd_operation_id");
			HasMany(x => x.Goods).Cascade.AllDeleteOrphan().Inverse().KeyColumn("order_upd_operation_id");
		}
	}
}
