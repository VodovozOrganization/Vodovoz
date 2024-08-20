using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contracts
{
	public class CounterpartyContractMap : ClassMap<CounterpartyContract>
	{
		public CounterpartyContractMap()
		{
			Table("counterparty_contract");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.MaxDelay).Column("max_delay");
			Map(x => x.IssueDate).Column("issue_date");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.OnCancellation).Column("on_cancellation");
			Map(x => x.Number).Column("number");
			Map(x => x.ContractSubNumber).Column("subnumber");
			Map(x => x.ChangedTemplateFile).Column("doc_changed_template").LazyLoad();
			Map(x => x.ContractType).Column("contract_type");
			References(x => x.DocumentTemplate).Column("doc_template_id");
			References(x => x.Organization).Column("organization_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}

