using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromWarehouseDocumentItemMap : SubclassMap<InstanceWriteOffFromWarehouseDocumentItem>
	{
		public InstanceWriteOffFromWarehouseDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromWarehouseDocumentItem));
		}
	}
}
