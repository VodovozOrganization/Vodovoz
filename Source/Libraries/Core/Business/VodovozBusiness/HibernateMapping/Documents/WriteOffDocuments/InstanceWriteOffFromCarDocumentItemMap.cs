using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromCarDocumentItemMap : SubclassMap<InstanceWriteOffFromCarDocumentItem>
	{
		public InstanceWriteOffFromCarDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromCarDocumentItem));
		}
	}
}
