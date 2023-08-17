using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromCarDocumentItemMap : SubclassMap<InstanceWriteOffFromCarDocumentItem>
	{
		public InstanceWriteOffFromCarDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromCarDocumentItem));
		}
	}
}
