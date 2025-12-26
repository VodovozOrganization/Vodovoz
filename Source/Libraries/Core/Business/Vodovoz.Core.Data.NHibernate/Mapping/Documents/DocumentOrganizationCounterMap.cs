using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Documents
{
	public class DocumentOrganizationCounterMap  : ClassMap<DocumentOrganizationCounter>
	{
		public DocumentOrganizationCounterMap()
		{
			Table("document_organization_counters");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.DocumentNumber).Column("document_number");
			Map(x => x.DocumentType).Column("document_type");
			Map(x => x.Counter).Column("counter");
			Map(x => x.CounterDate).Column("counter_date");
			
			References(x => x.Organization).Column("organization_id");
			References(x => x.Order).Column("order_id");
		}
	}
}
