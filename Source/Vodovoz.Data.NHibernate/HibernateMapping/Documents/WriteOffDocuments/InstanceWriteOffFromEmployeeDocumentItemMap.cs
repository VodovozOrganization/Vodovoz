using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromEmployeeDocumentItemMap : SubclassMap<InstanceWriteOffFromEmployeeDocumentItem>
	{
		public InstanceWriteOffFromEmployeeDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromEmployeeDocumentItem));
		}
	}
}
