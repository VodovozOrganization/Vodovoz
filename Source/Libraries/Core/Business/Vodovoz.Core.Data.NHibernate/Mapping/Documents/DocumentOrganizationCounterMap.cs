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
			Map(x => x.Counter).Column("counter");
			Map(x => x.CounterDateYear).Column("counter_date_year");
			
			References(x => x.Organization).Column("organization_id");
		}
	}
}
