using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffFromWarehouseDocumentItemMap : SubclassMap<InstanceWriteOffFromWarehouseDocumentItem>
	{
		public InstanceWriteOffFromWarehouseDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.InstanceWriteOffFromWarehouseDocumentItem));
		}
	}
}
