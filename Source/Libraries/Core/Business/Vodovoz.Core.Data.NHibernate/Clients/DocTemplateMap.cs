using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class DocTemplateMap : ClassMap<DocTemplateEntity>
	{
		public DocTemplateMap()
		{
			Table("doc_templates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.TempalteFile).Column("file").LazyLoad();
			Map(x => x.TemplateType).Column("type");
			Map(x => x.ContractType).Column("contract_type");
			References(x => x.Organization).Column("organization_id");
		}
	}
}
