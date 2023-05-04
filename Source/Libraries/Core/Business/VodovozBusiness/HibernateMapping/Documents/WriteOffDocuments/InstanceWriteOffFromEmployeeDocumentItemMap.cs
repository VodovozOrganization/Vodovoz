using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromEmployeeDocumentItemMap : SubclassMap<InstanceWriteOffFromEmployeeDocumentItem>
	{
		public InstanceWriteOffFromEmployeeDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromEmployeeDocumentItem));
		}
	}
}
