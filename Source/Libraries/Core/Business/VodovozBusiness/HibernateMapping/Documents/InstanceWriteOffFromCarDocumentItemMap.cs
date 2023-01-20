using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class InstanceWriteOffFromCarDocumentItemMap : SubclassMap<InstanceWriteOffFromCarDocumentItem>
	{
		public InstanceWriteOffFromCarDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Instance));
		}
	}
}
